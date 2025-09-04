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

namespace Application.Features.DeliveryManSection.Order.Queries
{
    public sealed record GetPendingOrdersWithinRadiusQuery : IRequest<Result<List<PendingOrderDto>>>
    {
        private class GetPendingOrdersWithinRadiusQueryHandler : IRequestHandler<GetPendingOrdersWithinRadiusQuery,
                                                                                Result<List<PendingOrderDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private const double RADIUS_IN_KM = 3.0;
            private const double EARTH_RADIUS_KM = 6371.0; // Earth's radius in kilometers

            public GetPendingOrdersWithinRadiusQueryHandler(INaqlahContext context,
                                                           IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<List<PendingOrderDto>>> Handle(GetPendingOrdersWithinRadiusQuery request, 
                                                                   CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                
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
                    return Result.Failure<List<PendingOrderDto>>("Delivery man not found");
                }

                if (deliveryMan.DeliveryManLocation == null)
                {
                    return Result.Failure<List<PendingOrderDto>>("Delivery man location not available");
                }

                if (deliveryMan.Vehicle == null || deliveryMan.Vehicle.VehicleType == null)
                {
                    return Result.Failure<List<PendingOrderDto>>("Delivery man vehicle information not available");
                }

                // Get the categories that the delivery man's vehicle can handle
                var vehicleCategoryIds = deliveryMan.Vehicle.VehicleType.VehicleTypeCategoies
                    .Select(vtc => vtc.MainCategoryId)
                    .ToList();

                if (!vehicleCategoryIds.Any())
                {
                    return Result.Success(new List<PendingOrderDto>());
                }

                var deliveryManLat = deliveryMan.DeliveryManLocation.Latitude;
                var deliveryManLon = deliveryMan.DeliveryManLocation.Longitude;

                // Get pending orders that match the vehicle's capabilities
                var pendingOrders = await context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.OrderWayPoints)
                    .Where(o => o.OrderStatus == OrderStatus.Pending && 
                               o.DeliveryManId == null &&
                               o.OrderDetails.All(od => vehicleCategoryIds.Contains(od.MainCategoryId)))
                    .ToListAsync(cancellationToken);

                var ordersWithinRadius = new List<PendingOrderDto>();

                foreach (var order in pendingOrders)
                {
                    // Get origin waypoint (pickup location)
                    var originWaypoint = order.OrderWayPoints.FirstOrDefault(wp => wp.IsOrgin);
                    if (originWaypoint == null)
                        continue;

                    // Calculate distance using Haversine formula
                    var distance = CalculateDistance(deliveryManLat, deliveryManLon, 
                                                    originWaypoint.Latitude, originWaypoint.longitude);

                    if (distance <= RADIUS_IN_KM)
                    {
                        // Get destination waypoint (delivery location)
                        var destinationWaypoint = order.OrderWayPoints.FirstOrDefault(wp => wp.IsDestination);

                        var pendingOrderDto = new PendingOrderDto
                        {
                            Id = order.Id,
                            OrderNumber = order.OrderNumber,
                            Total = order.Total,
                            Distance = Math.Round(distance, 2),
                            PickupLatitude = originWaypoint.Latitude,
                            PickupLongitude = originWaypoint.longitude,
                            DeliveryLatitude = destinationWaypoint?.Latitude ?? 0,
                            DeliveryLongitude = destinationWaypoint?.longitude ?? 0,
                            Categories = order.OrderDetails.Select(od => 
                                languageId == (int)Language.Arabic ? 
                                od.ArabicCategoryName : od.EnglishCategoryName).ToList(),
                            CreatedAt = DateTime.Now // You might want to add a CreatedAt field to Order model
                        };

                        // Get address information
                        if (originWaypoint.NeighborhoodId > 0 && originWaypoint.CityId > 0)
                        {
                            var region = await context.Regions
                                .Include(r => r.Cities)
                                    .ThenInclude(c => c.Neighborhoods)
                                .Where(r => r.Id == originWaypoint.RegionId)
                                .FirstOrDefaultAsync(cancellationToken);
                            
                            if (region != null)
                            {
                                var city = region.Cities.FirstOrDefault(c => c.Id == originWaypoint.CityId);
                                var neighborhood = city?.Neighborhoods.FirstOrDefault(n => n.Id == originWaypoint.NeighborhoodId);
                                
                                if (neighborhood != null && city != null)
                                {
                                    pendingOrderDto.PickupAddress = languageId == (int)Language.Arabic ?
                                        $"{neighborhood.ArabicName}, {city.ArabicName}" :
                                        $"{neighborhood.EnglishName}, {city.EnglishName}";
                                }
                            }
                        }

                        if (destinationWaypoint != null && destinationWaypoint.NeighborhoodId > 0 && destinationWaypoint.CityId > 0)
                        {
                            var region = await context.Regions
                                .Include(r => r.Cities)
                                    .ThenInclude(c => c.Neighborhoods)
                                .Where(r => r.Id == destinationWaypoint.RegionId)
                                .FirstOrDefaultAsync(cancellationToken);
                            
                            if (region != null)
                            {
                                var city = region.Cities.FirstOrDefault(c => c.Id == destinationWaypoint.CityId);
                                var neighborhood = city?.Neighborhoods.FirstOrDefault(n => n.Id == destinationWaypoint.NeighborhoodId);
                                
                                if (neighborhood != null && city != null)
                                {
                                    pendingOrderDto.DeliveryAddress = languageId == (int)Language.Arabic ?
                                        $"{neighborhood.ArabicName}, {city.ArabicName}" :
                                        $"{neighborhood.EnglishName}, {city.EnglishName}";
                                }
                            }
                        }

                        ordersWithinRadius.Add(pendingOrderDto);
                    }
                }

                // Sort by distance (closest first)
                ordersWithinRadius = ordersWithinRadius.OrderBy(o => o.Distance).ToList();

                return Result.Success(ordersWithinRadius);
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