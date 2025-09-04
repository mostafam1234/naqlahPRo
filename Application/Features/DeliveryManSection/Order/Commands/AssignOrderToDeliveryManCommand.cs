using Application.Features.DeliveryManSection.Order.Dtos;
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

namespace Application.Features.DeliveryManSection.Order.Commands
{
    public sealed record AssignOrderToDeliveryManCommand : IRequest<Result<bool>>
    {
        public AssignOrderRequestDto Request { get; init; }

        private class AssignOrderToDeliveryManCommandHandler : IRequestHandler<AssignOrderToDeliveryManCommand,
                                                                              Result<bool>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IDateTimeProvider dateTimeProvider;
            private const double RADIUS_IN_KM = 3.0;
            private const double EARTH_RADIUS_KM = 6371.0;

            public AssignOrderToDeliveryManCommandHandler(INaqlahContext context,
                                                         IUserSession userSession,
                                                         IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.userSession = userSession;
                this.dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result<bool>> Handle(AssignOrderToDeliveryManCommand request,
                                                  CancellationToken cancellationToken)
            {
                // Get delivery man with their location and vehicle details
                var deliveryMan = await context.DeliveryMen
                    .Include(dm => dm.DeliveryManLocation)
                    .Include(dm => dm.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                            .ThenInclude(vt => vt.VehicleTypeCategoies)
                    .Where(dm => dm.UserId == userSession.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryMan == null)
                {
                    return Result.Failure<bool>("Delivery man not found");
                }

                if (!deliveryMan.Active)
                {
                    return Result.Failure<bool>("Delivery man is not active");
                }

                if (deliveryMan.DeliveryManLocation == null)
                {
                    return Result.Failure<bool>("Delivery man location not available");
                }

                if (deliveryMan.Vehicle == null || deliveryMan.Vehicle.VehicleType == null)
                {
                    return Result.Failure<bool>("Delivery man vehicle information not available");
                }

                // Get the order with its details
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.OrderWayPoints)
                    .Where(o => o.Id == request.Request.OrderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (order == null)
                {
                    return Result.Failure<bool>("Order not found");
                }

                // Verify that the delivery man's vehicle can handle all the order's categories
                var vehicleCategoryIds = deliveryMan.Vehicle.VehicleType.VehicleTypeCategoies
                    .Select(vtc => vtc.MainCategoryId)
                    .ToList();

                var orderCategoryIds = order.OrderDetails
                    .Select(od => od.MainCategoryId)
                    .ToList();

                if (!orderCategoryIds.All(categoryId => vehicleCategoryIds.Contains(categoryId)))
                {
                    return Result.Failure<bool>("Your vehicle type does not support all the order categories");
                }

                // Verify that the order is within the 3km radius
                var originWaypoint = order.OrderWayPoints.FirstOrDefault(wp => wp.IsOrgin);
                if (originWaypoint == null)
                {
                    return Result.Failure<bool>("Order origin waypoint not found");
                }

                var distance = CalculateDistance(
                    deliveryMan.DeliveryManLocation.Latitude,
                    deliveryMan.DeliveryManLocation.Longitude,
                    originWaypoint.Latitude,
                    originWaypoint.longitude);

                if (distance > RADIUS_IN_KM)
                {
                    return Result.Failure<bool>("Order is outside your service radius");
                }

                // Assign the order to the delivery man using domain logic
                var assignmentResult = order.AssignToDeliveryMan(deliveryMan.Id, dateTimeProvider.Now);
                if (assignmentResult.IsFailure)
                {
                    return Result.Failure<bool>(assignmentResult.Error);
                }

                // Save changes
                var saveResult = await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<bool>(saveResult.Error);
                }

                return Result.Success(true);
            }

            private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
            {
                // Haversine formula to calculate distance between two coordinates
                var dLat = ToRadians(lat2 - lat1);
                var dLon = ToRadians(lon2 - lon1);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = EARTH_RADIUS_KM * c;

                return distance;
            }

            private double ToRadians(double degrees)
            {
                return degrees * (Math.PI / 180);
            }
        }
    }
}