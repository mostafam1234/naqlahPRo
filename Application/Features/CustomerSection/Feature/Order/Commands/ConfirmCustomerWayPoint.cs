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
    public sealed record ConfirmCustomerWayPoint:IRequest<Result>
    {
        public int OrderId { get; set; }
        public int OrderWayPointId { get; set; }

        private class ConfirmCustomerWayPointHandler : IRequestHandler<ConfirmCustomerWayPoint, Result>
        {
            private readonly INaqlahContext naqlahContext;
            private readonly INotificationService notificationService;
            private readonly IDateTimeProvider dateTimeProvider;

            public ConfirmCustomerWayPointHandler(INaqlahContext naqlahContext,
                                                  INotificationService notificationService,
                                                  IDateTimeProvider dateTimeProvider )
            {
                this.naqlahContext = naqlahContext;
                this.notificationService = notificationService;
                this.dateTimeProvider = dateTimeProvider;
            }
            public async Task<Result> Handle(ConfirmCustomerWayPoint request, CancellationToken cancellationToken)
            {
                var nowDate = dateTimeProvider.Now;
                var order = await naqlahContext.Orders
                                             .Include(x => x.OrderWayPoints)
                                             .AsTracking()
                                             .FirstOrDefaultAsync(x => x.Id == request.OrderId);


                if (order == null)
                {
                    return Result.Failure("Order not found");
                }

                var result = order.ConfirmOrderWayPointFromCustomer(request.OrderWayPointId,nowDate);
                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }


                var checkCompleteResult = order.CheckAndCompleteIfAllWayPointsPickedUp(nowDate);

                if (checkCompleteResult.IsFailure)
                {
                    return Result.Failure(checkCompleteResult.Error);
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
                        Title = "Confirm Way Point",
                        Body = $" order #{order.OrderNumber} is Way Point Confirmed",
                        FireBaseTokens = firebaseTokens.Where(x=>!string.IsNullOrEmpty(x)).ToList(),
                        PayLoad = new Dictionary<string, string>
                        {
                            { "orderId", order.Id.ToString() },
                            { "orderNumber", order.OrderNumber },
                            { "orderWayPointId", request.OrderWayPointId.ToString()},
                            { "type", ((int)NotificationType.PickUp).ToString() }
                        }
                    };

                    await notificationService.SendNotificationAsyncToMultipleDevices(notificationBody);
                }

                return Result.Success();
            }
        }
    }
}
