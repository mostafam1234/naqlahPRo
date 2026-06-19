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
    public sealed record ConfirmPickupByDriverCommand : IRequest<Result>
    {
        public int OrderId { get; init; }

        private class Handler : IRequestHandler<ConfirmPickupByDriverCommand, Result>
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

            public async Task<Result> Handle(ConfirmPickupByDriverCommand request, CancellationToken cancellationToken)
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

                var pickupResult = order.ConfirmPickupByDriver(dateTimeProvider.Now);
                if (pickupResult.IsFailure)
                {
                    return Result.Failure(pickupResult.Error);
                }

                // Pickup remains pending until the client approves the handoff/photo.
                await customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تم التقاط الشحنة",
                    "Shipment Picked Up",
                    $"التقط الكابتن شحنتك رقم {order.OrderNumber}، يرجى تأكيد الاستلام",
                    $"The captain picked up your order #{order.OrderNumber}, please approve the pickup",
                    NotificationType.OrderStatusUpdate,
                    cancellationToken);

                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
