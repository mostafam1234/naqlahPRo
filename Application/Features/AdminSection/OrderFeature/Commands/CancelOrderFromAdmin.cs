using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public class CancelOrderFromAdmin : IRequest<Result<int>>
    {
        public int OrderId { get; init; }
        public int LanguageId { get; init; } = 1; // Default to Arabic

        private class CancelOrderFromAdminHandler : IRequestHandler<CancelOrderFromAdmin, Result<int>>
        {
            private readonly INaqlahContext context;
            private readonly ICustomerNotificationService customerNotificationService;
            private readonly IDateTimeProvider dateTimeProvider;

            public CancelOrderFromAdminHandler(INaqlahContext context,
                ICustomerNotificationService customerNotificationService,
                IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.customerNotificationService = customerNotificationService;
                this.dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result<int>> Handle(CancelOrderFromAdmin request, CancellationToken cancellationToken)
            {
                var order = await context.Orders
                    .AsTracking()
                    .Include(o => o.OrderStatusHistories)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order == null)
                    return Result.Failure<int>("OrderNotFound");

                if (order.OrderStatus == OrderStatus.Cancelled)
                    return Result.Failure<int>("OrderAlreadyCancelled");

                if (order.OrderStatus != OrderStatus.Pending)
                    return Result.Failure<int>("OnlyPendingOrdersCanBeCanceled");

                order.CancelOrder(dateTimeProvider.Now);

                await customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تم إلغاء الطلب",
                    "Order Cancelled",
                    $"تم إلغاء طلبك رقم {order.OrderNumber}",
                    $"Your order #{order.OrderNumber} has been cancelled",
                    NotificationType.OrderCancelled,
                    cancellationToken);

                await context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }

    }
}