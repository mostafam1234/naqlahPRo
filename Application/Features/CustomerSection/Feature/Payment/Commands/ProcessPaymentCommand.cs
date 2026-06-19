using Application.Features.CustomerSection.Feature.Payment.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderModel = Domain.Models.Order;
using DiscountCodeModel = Domain.Models.DiscountCode;

namespace Application.Features.CustomerSection.Feature.Payment.Commands
{
    public sealed record ProcessPaymentCommand : IRequest<Result<ProcessPaymentResponseDto>>
    {
        public int OrderId { get; init; }
        public int PaymentMethodId { get; init; }
        public string? DiscountCode { get; init; }

        private class Handler : IRequestHandler<ProcessPaymentCommand, Result<ProcessPaymentResponseDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IMadaPaymentService _madaPaymentService;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly ILogger<Handler> _logger;

            public Handler(
                INaqlahContext context,
                IMadaPaymentService madaPaymentService,
                IDateTimeProvider dateTimeProvider,
                ILogger<Handler> logger)
            {
                _context = context;
                _madaPaymentService = madaPaymentService;
                _dateTimeProvider = dateTimeProvider;
                _logger = logger;
            }

            public async Task<Result<ProcessPaymentResponseDto>> Handle(
                ProcessPaymentCommand request,
                CancellationToken cancellationToken)
            {
                var order = await _context.Orders
                    .AsTracking()
                    .Include(o => o.PaymentMethods)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order is null)
                    return Result.Failure<ProcessPaymentResponseDto>("Order not found.");

                if (order.OrderStatus == OrderStatus.Cancelled)
                    return Result.Failure<ProcessPaymentResponseDto>("Cannot process payment for a cancelled order.");

                // Validate and apply discount — updates order.Total in the domain
                if (!string.IsNullOrWhiteSpace(request.DiscountCode))
                {
                    var discount = await _context.DiscountCodes
                        .AsTracking()
                        .FirstOrDefaultAsync(
                            x => x.Code == request.DiscountCode.Trim().ToUpper(),
                            cancellationToken);

                    if (discount is null)
                        return Result.Failure<ProcessPaymentResponseDto>("Invalid discount code.");

                    var validationResult = ValidateDiscount(discount, order.Total);
                    if (validationResult.IsFailure)
                        return Result.Failure<ProcessPaymentResponseDto>(validationResult.Error);

                    var applyResult = order.ApplyDiscount(discount.Id, discount.Code, discount.DiscountAmount);
                    if (applyResult.IsFailure)
                        return Result.Failure<ProcessPaymentResponseDto>(applyResult.Error);

                    // Increment usage count on the discount code
                    var incrementResult = discount.IncrementUsage();
                    if (incrementResult.IsFailure)
                        return Result.Failure<ProcessPaymentResponseDto>(incrementResult.Error);
                }

                // order.Total now reflects the final amount (after discount)
                return request.PaymentMethodId switch
                {
                    (int)PaymentMethodEnum.Online  => await HandleMadaPaymentAsync(order, cancellationToken),
                    (int)PaymentMethodEnum.Cash    => await HandleCodPaymentAsync(order, cancellationToken),
                    (int)PaymentMethodEnum.Wallet  => await HandleWalletPaymentAsync(order, cancellationToken),
                    _ => Result.Failure<ProcessPaymentResponseDto>(
                            $"Unsupported payment method: {request.PaymentMethodId}.")
                };
            }

            // ── Mada (HyperPay) ──────────────────────────────────────────────────────

            private async Task<Result<ProcessPaymentResponseDto>> HandleMadaPaymentAsync(
                OrderModel order,
                CancellationToken cancellationToken)
            {
                var checkoutResult = await _madaPaymentService.InitiateCheckoutAsync(
                    order.Id, order.Total, cancellationToken: cancellationToken);

                if (!checkoutResult.Success)
                {
                    _logger.LogError(
                        "Mada checkout initiation failed for order {OrderId}: {Error}",
                        order.Id, checkoutResult.ErrorMessage);
                    return Result.Failure<ProcessPaymentResponseDto>(
                        $"Payment initiation failed: {checkoutResult.ErrorMessage}");
                }

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<ProcessPaymentResponseDto>(saveResult.Error);

                return Result.Success(new ProcessPaymentResponseDto
                {
                    Success = true,
                    Message = "Mada checkout session created. Complete payment using the provided URL.",
                    TransactionId = checkoutResult.CheckoutId,
                    CheckoutId = checkoutResult.CheckoutId,
                    CheckoutUrl = checkoutResult.CheckoutUrl
                });
            }

            // ── COD ──────────────────────────────────────────────────────────────────

            private async Task<Result<ProcessPaymentResponseDto>> HandleCodPaymentAsync(
                OrderModel order,
                CancellationToken cancellationToken)
            {
                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<ProcessPaymentResponseDto>(saveResult.Error);

                return Result.Success(new ProcessPaymentResponseDto
                {
                    Success = true,
                    Message = "Cash on delivery confirmed. Payment will be collected at pickup.",
                    TransactionId = $"COD-{order.Id}-{_dateTimeProvider.Now:yyyyMMddHHmmss}"
                });
            }

            // ── Wallet ───────────────────────────────────────────────────────────────

            private async Task<Result<ProcessPaymentResponseDto>> HandleWalletPaymentAsync(
                OrderModel order,
                CancellationToken cancellationToken)
            {
                var walletBalance = await _context.WalletTransctions
                    .Where(x => x.CustomerId == order.CustomerId)
                    .SumAsync(x => x.Withdraw ? -x.Amount : x.Amount, cancellationToken);

                if (walletBalance < order.Total)
                    return Result.Failure<ProcessPaymentResponseDto>("Insufficient wallet balance.");

                var nowDate = _dateTimeProvider.Now;
                var walletTransaction = Domain.Models.WalletTransctions.Instance(
                    nowDate,
                    $"تسديد تكلفة الطلب رقم {order.OrderNumber}",
                    $"Payment for order {order.OrderNumber}",
                    order.CustomerId,
                    order.Total,
                    true,
                    order.Id);

                if (walletTransaction.IsFailure)
                    return Result.Failure<ProcessPaymentResponseDto>(walletTransaction.Error);

                await _context.WalletTransctions.AddAsync(walletTransaction.Value, cancellationToken);

                order.MarkAsPaid();

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<ProcessPaymentResponseDto>(saveResult.Error);

                return Result.Success(new ProcessPaymentResponseDto
                {
                    Success = true,
                    Message = "Payment deducted from wallet successfully.",
                    TransactionId = $"WAL-{order.Id}-{nowDate:yyyyMMddHHmmss}"
                });
            }

            // ── Helpers ──────────────────────────────────────────────────────────────

            private static Result ValidateDiscount(DiscountCodeModel discount, decimal orderTotal)
            {
                if (!discount.IsActive)
                    return Result.Failure("This discount code is no longer active.");

                if (discount.ExpiryDate.HasValue && discount.ExpiryDate.Value < DateTime.UtcNow)
                    return Result.Failure("This discount code has expired.");

                if (discount.UsageLimit.HasValue && discount.UsageCount >= discount.UsageLimit.Value)
                    return Result.Failure("This discount code has reached its usage limit.");

                if (discount.MinimumOrderAmount.HasValue && orderTotal < discount.MinimumOrderAmount.Value)
                    return Result.Failure(
                        $"Order total must be at least {discount.MinimumOrderAmount.Value:F2} to use this code.");

                return Result.Success();
            }
        }
    }
}
