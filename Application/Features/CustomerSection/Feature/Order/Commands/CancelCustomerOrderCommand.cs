using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record CancelCustomerOrderCommand : IRequest<Result<CancelOrderResponseDto>>
    {
        public int OrderId { get; set; }
        public string? Iban { get; set; }


        public class CancelCustomerOrderCommandHandler : IRequestHandler<CancelCustomerOrderCommand, Result<CancelOrderResponseDto>>
        {
            private readonly INaqlahContext context;
            private readonly IDateTimeProvider dateTimeProvider;
            private readonly ICustomerNotificationService customerNotificationService;

            public CancelCustomerOrderCommandHandler(INaqlahContext context, IDateTimeProvider dateTimeProvider,
                ICustomerNotificationService customerNotificationService)
            {
                this.context = context;
                this.dateTimeProvider = dateTimeProvider;
                this.customerNotificationService = customerNotificationService;
            }
            public async Task<Result<CancelOrderResponseDto>> Handle(CancelCustomerOrderCommand request, CancellationToken cancellationToken)
            {
                var order = await context.Orders.AsTracking().Include(x=>x.OrderStatusHistories).FirstOrDefaultAsync(x=>x.Id==request.OrderId, cancellationToken);
                if (order == null)
                {
                    return Result.Failure<CancelOrderResponseDto>("Order not found");
                }

                var cancelResult = order.CancelOrder(dateTimeProvider.Now);
                if (cancelResult.IsFailure)
                {
                    return Result.Failure<CancelOrderResponseDto>(cancelResult.Error);
                }

                // Paid orders require an IBAN so the refund can be initiated (settled within 2 business days).
                var refundInitiated = false;
                if (order.IsPaid)
                {
                    var refundResult = order.InitiateRefund(request.Iban ?? string.Empty);
                    if (refundResult.IsFailure)
                    {
                        return Result.Failure<CancelOrderResponseDto>(refundResult.Error);
                    }

                    refundInitiated = true;

                    await customerNotificationService.PrepareAsync(
                        order.CustomerId,
                        order.Id,
                        "تم بدء استرداد المبلغ",
                        "Refund Initiated",
                        $"تم بدء استرداد مبلغ طلبك رقم {order.OrderNumber} وسيتم تحويله خلال يومي عمل",
                        $"A refund for your order #{order.OrderNumber} has been initiated and will be transferred within 2 business days",
                        NotificationType.OrderCancelled,
                        cancellationToken);
                }

                await customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تم إلغاء الطلب",
                    "Order Cancelled",
                    $"تم إلغاء طلبك رقم {order.OrderNumber}",
                    $"Your order #{order.OrderNumber} has been cancelled",
                    NotificationType.OrderCancelled,
                    cancellationToken);

                var saveResult=await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<CancelOrderResponseDto>(saveResult.Error);
                }

                return Result.Success(new CancelOrderResponseDto
                {
                    Success = true,
                    Message = "Order cancelled",
                    RefundInitiated = refundInitiated,
                    RefundStatus = refundInitiated ? order.RefundStatus.ToString().ToLowerInvariant() : null
                });
            }
        }
    }
}
