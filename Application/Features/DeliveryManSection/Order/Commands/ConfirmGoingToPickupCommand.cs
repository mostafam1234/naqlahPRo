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
    public sealed record ConfirmGoingToPickupCommand : IRequest<Result>
    {
        public int OrderId { get; init; }

        private class Handler : IRequestHandler<ConfirmGoingToPickupCommand, Result>
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

            public async Task<Result> Handle(ConfirmGoingToPickupCommand request, CancellationToken cancellationToken)
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

                var confirmResult = order.ConfirmGoingToPickup(dateTimeProvider.Now);
                if (confirmResult.IsFailure)
                {
                    return Result.Failure(confirmResult.Error);
                }

                await customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تأكيد الذهاب لالتقاط الشحنة",
                    "Captain Going To Pickup",
                    $"أكد الكابتن توجهه لالتقاط شحنتك رقم {order.OrderNumber}",
                    $"The captain confirmed going to pick up your order #{order.OrderNumber}",
                    NotificationType.OrderStatusUpdate,
                    cancellationToken);

                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
