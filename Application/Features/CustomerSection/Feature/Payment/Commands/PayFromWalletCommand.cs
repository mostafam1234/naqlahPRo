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
    public sealed record PayFromWalletCommand : IRequest<Result<PayFromWalletResponseDto>>
    {
        public int OrderId { get; init; }
        public string? DiscountCode { get; init; }

        private class Handler : IRequestHandler<PayFromWalletCommand, Result<PayFromWalletResponseDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly ILogger<Handler> _logger;

            public Handler(
                INaqlahContext context,
                IUserSession userSession,
                IDateTimeProvider dateTimeProvider,
                ILogger<Handler> logger)
            {
                _context = context;
                _userSession = userSession;
                _dateTimeProvider = dateTimeProvider;
                _logger = logger;
            }

            public async Task<Result<PayFromWalletResponseDto>> Handle(
                PayFromWalletCommand request,
                CancellationToken cancellationToken)
            {
                // Resolve customer from the logged-in user
                var customerId = await _context.Customers
                    .Where(x => x.UserId == _userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                    return Result.Failure<PayFromWalletResponseDto>("Customer not found.");

                // Load order and verify ownership
                var order = await _context.Orders
                    .AsTracking()
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order is null)
                    return Result.Failure<PayFromWalletResponseDto>("Order not found.");

                if (order.CustomerId != customerId)
                    return Result.Failure<PayFromWalletResponseDto>("Order does not belong to the current customer.");

                if (order.OrderStatus == OrderStatus.Cancelled)
                    return Result.Failure<PayFromWalletResponseDto>("Cannot pay for a cancelled order.");

                if (order.OrderStatus == OrderStatus.Completed)
                    return Result.Failure<PayFromWalletResponseDto>("Order is already completed.");

                // Apply discount code if provided — updates order.Total in the domain
                if (!string.IsNullOrWhiteSpace(request.DiscountCode))
                {
                    var discount = await _context.DiscountCodes
                        .AsTracking()
                        .FirstOrDefaultAsync(
                            x => x.Code == request.DiscountCode.Trim().ToUpper(),
                            cancellationToken);

                    if (discount is null)
                        return Result.Failure<PayFromWalletResponseDto>("Invalid discount code.");

                    var validationResult = ValidateDiscount(discount, order.Total);
                    if (validationResult.IsFailure)
                        return Result.Failure<PayFromWalletResponseDto>(validationResult.Error);

                    var applyResult = order.ApplyDiscount(discount.Id, discount.Code, discount.DiscountAmount);
                    if (applyResult.IsFailure)
                        return Result.Failure<PayFromWalletResponseDto>(applyResult.Error);

                    var incrementResult = discount.IncrementUsage();
                    if (incrementResult.IsFailure)
                        return Result.Failure<PayFromWalletResponseDto>(incrementResult.Error);
                }

                // Load all wallet transactions to compute current balance
                var transactions = await _context.WalletTransctions
                    .Where(x => x.CustomerId == customerId)
                    .ToListAsync(cancellationToken);

                var currentBalance = transactions.Any()
                    ? transactions.Sum(x => x.Withdraw ? -x.Amount : x.Amount)
                    : 0m;

                if (currentBalance < order.Total)
                    return Result.Failure<PayFromWalletResponseDto>(
                        $"Insufficient wallet balance. Available: {currentBalance:F2}, Required: {order.Total:F2}.");

                // Create the withdrawal transaction
                var nowDate = _dateTimeProvider.Now;
                var walletTransaction = Domain.Models.WalletTransctions.Instance(
                    nowDate,
                    $"تسديد تكلفة الطلب رقم {order.OrderNumber}",
                    $"Payment for order {order.OrderNumber}",
                    customerId,
                    order.Total,
                    true,
                    order.Id);

                if (walletTransaction.IsFailure)
                    return Result.Failure<PayFromWalletResponseDto>(walletTransaction.Error);

                await _context.WalletTransctions.AddAsync(walletTransaction.Value, cancellationToken);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<PayFromWalletResponseDto>(saveResult.Error);

                var newBalance = currentBalance - order.Total;

                _logger.LogInformation(
                    "Wallet payment of {Amount} SAR processed for order {OrderId}. New balance: {Balance}",
                    order.Total, order.Id, newBalance);

                return Result.Success(new PayFromWalletResponseDto
                {
                    Success = true,
                    Message = "Paid from wallet successfully.",
                    NewBalance = newBalance
                });
            }

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
