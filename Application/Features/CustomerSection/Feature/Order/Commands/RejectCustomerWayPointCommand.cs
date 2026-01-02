using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record RejectCustomerWayPointCommand:IRequest<Result>
    {
        public int OrderId { get; set; }
        public int OrderWayPointId { get; set; }

        private class RejectCustomerWayPointCommandHandler : IRequestHandler<RejectCustomerWayPointCommand, Result>
        {
            private readonly INaqlahContext naqlahContext;
            private readonly INotificationService notificationService;

            public RejectCustomerWayPointCommandHandler(INaqlahContext naqlahContext, INotificationService notificationService)
            {
                this.naqlahContext = naqlahContext;
                this.notificationService = notificationService;
            }
            public async Task<Result> Handle(RejectCustomerWayPointCommand request, CancellationToken cancellationToken)
            {
                var order = await naqlahContext.Orders
                                             .Include(x => x.OrderWayPoints)
                                             .AsTracking()
                                             .FirstOrDefaultAsync(x => x.Id == request.OrderId);


                if (order == null)
                {
                    return Result.Failure("Order not found");
                }

                var result = order.RejectOrderWayPointFromCustomer(request.OrderWayPointId);
                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }

                var saveResult = await naqlahContext.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure(saveResult.Error);
                }
                var deliveryMan = await naqlahContext.DeliveryMen
                                                                .Include(x => x.User)
                                                                .FirstOrDefaultAsync(x => x.Id == order.DeliveryManId);
                if (deliveryMan != null)
                {
                    var firebaseTokens = new List<string>
                    {
                        deliveryMan.AndriodDevice,
                        deliveryMan.IosDevice
                    };


                    var notificationBody = new NotificationBodyForMultipleDevices
                    {
                        Title = "Order Way Point Rejected",
                        Body = $" order #{order.OrderNumber} is Way Point Rejected",
                        FireBaseTokens = firebaseTokens.Where(x => !string.IsNullOrEmpty(x)).ToList(),
                        PayLoad = new Dictionary<string, string>
                        {
                            { "orderId", order.Id.ToString() },
                            { "orderNumber", order.OrderNumber },
                            { "orderWayPointId", request.OrderWayPointId.ToString()},
                            { "type", ((int)NotificationType.RejectORderWayPointFromCustomer).ToString() }
                        }
                    };

                    await notificationService.SendNotificationAsyncToMultipleDevices(notificationBody);
                }

                return Result.Success();
            }
        }
    }
}
