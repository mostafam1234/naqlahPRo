using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Order.Commands
{
    public sealed record CancelOrderByDriverCommand : IRequest<Result>
    {
        public int OrderId { get; init; }

        private class Handler : IRequestHandler<CancelOrderByDriverCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IDateTimeProvider dateTimeProvider;
            private readonly ICustomerNotificationService customerNotificationService;

            public Handler(INaqlahContext context,
                           IUserSession userSession,
                           IDateTimeProvider dateTimeProvider,
                           ICustomerNotificationService customerNotificationService)
            {
                this.context = context;
                this.userSession = userSession;
                this.dateTimeProvider = dateTimeProvider;
                this.customerNotificationService = customerNotificationService;
            }

            public async Task<Result> Handle(CancelOrderByDriverCommand request, CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure("Delivery man not found");
                }

                var order = await context.Orders
                    .AsTracking()
                    .Include(x => x.OrderStatusHistories)
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

                if (order is null)
                {
                    return Result.Failure("Order not found");
                }

                if (order.DeliveryManId != deliveryManId)
                {
                    return Result.Failure("Order is not assigned to you");
                }

                // Driver cancel = reassign back to pending (not a client cancellation / refund flow).
                var cancelResult = order.CancelOrderByDriver(dateTimeProvider.Now);
                if (cancelResult.IsFailure)
                {
                    return Result.Failure(cancelResult.Error);
                }

                await customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "إعادة تعيين الطلب",
                    "Order Reassignment",
                    $"تم إلغاء ربط الكابتن بطلبك رقم {order.OrderNumber}، جارٍ إعادة التعيين",
                    $"The captain was unlinked from your order #{order.OrderNumber}, reassigning...",
                    NotificationType.OrderStatusUpdate,
                    cancellationToken);

                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
