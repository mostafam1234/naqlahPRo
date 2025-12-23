using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Services.GoogleMap;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record SelectVehicleTypeForOrderCommand(SelectVehicleTypeDto Request) : 
                         IRequest<Result<SelectVehicleTypeResponseDto>>
    {
        private class SelectVehicleTypeForOrderCommandHandler : IRequestHandler<SelectVehicleTypeForOrderCommand,
                                                                               Result<SelectVehicleTypeResponseDto>>
        {
            private readonly INaqlahContext context;
            private readonly INotificationService notificationService;
            private readonly IGoogleMapService googleMapService;
            private readonly ILogger<SelectVehicleTypeForOrderCommandHandler> logger;
            private const double RadiusInKilometers = 3.0;

            public SelectVehicleTypeForOrderCommandHandler(INaqlahContext context,
                                                          INotificationService notificationService,
                                                          IGoogleMapService googleMapService,
                                                          ILogger<SelectVehicleTypeForOrderCommandHandler> logger)
            {
                this.context = context;
                this.notificationService = notificationService;
                this.googleMapService = googleMapService;
                this.logger = logger;
            }

            public async Task<Result<SelectVehicleTypeResponseDto>> Handle(SelectVehicleTypeForOrderCommand request, 
                                                                           CancellationToken cancellationToken)
            {
                // Get the order with its details
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.OrderWayPoints)
                    .Include(o => o.OrderServices)
                    .FirstOrDefaultAsync(o => o.Id == request.Request.OrderId, cancellationToken);


                if (order == null)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>("Order not found");
                }

                var systemCOnfiguration = await context.SystemConfigurations.FirstOrDefaultAsync();
                if (systemCOnfiguration is null)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>("System Configuration must have value");
                }

                var googleResponse = await CalculateOrderDirection(order);
                if (googleResponse.IsFailure || googleResponse.Value.DistanceInKiloMeter == 0)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>("Invalid way Points Location");
                }

                var wallletTransctions = await context.WalletTransctions
                                                      .Where(x => x.CustomerId == order.CustomerId)
                                                      .ToListAsync();


                var customerBalance=wallletTransctions.Any()? wallletTransctions.Sum(x => x.Withdraw ? -x.Amount : x.Amount):0;


                // Get the origin waypoint (where IsOrgin = true)
                var originWaypoint = order.OrderWayPoints.FirstOrDefault(wp => wp.IsOrgin);
                if (originWaypoint == null)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>("Order origin waypoint not found");
                }

                var vehicleType=await context.VehicleTypes.FirstOrDefaultAsync();
                if (vehicleType is null)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>("Vehicle Could be Deleted");
                }

                // Set the vehicle type using domain logic
                var setVehicleResult = order.CompleteOrderAndSetVehicleType(request.Request.VehicleTypeId,
                                                                            vehicleType.Cost,
                                                                            customerBalance,
                                                                            systemCOnfiguration.BaseKm,
                                                                            systemCOnfiguration.BaseKmRate,
                                                                            systemCOnfiguration.ExtraKmRate,
                                                                            systemCOnfiguration.BaseHours,
                                                                            systemCOnfiguration.BaseHourRate,
                                                                            systemCOnfiguration.ExtraHourRate,
                                                                            systemCOnfiguration.VatRate,
                                                                            systemCOnfiguration.ServiceFess,
                                                                            (int)PaymentMethodEnum.Wallet,
                                                                            googleResponse.Value.DistanceInKiloMeter,
                                                                            googleResponse.Value.DeliveryTime,
                                                                            googleResponse.Value.EncodedPoints);


                if (setVehicleResult.IsFailure)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>(setVehicleResult.Error);
                }

                var arabicDescription = $"??? ?? ??????? ?? ??? ??? {order.OrderNumber}";
                var englishDescription = $"Cost Of Order Number {order.OrderNumber}";

                var walletTransction = WalletTransctions.Instance(arabicDescription,
                                                                 englishDescription,
                                                                 order.CustomerId,
                                                                 order.Total,
                                                                 true,
                                                                 order.Id);

                if (walletTransction.IsFailure)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>(walletTransction.Error);
                }

                await context.WalletTransctions.AddAsync(walletTransction.Value);

                // Save the changes
                var saveResult = await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<SelectVehicleTypeResponseDto>(saveResult.Error);
                }

                // Get all delivery men with the selected vehicle type and their locations
                var deliveryMenWithVehicle = await context.DeliveryMen
                    .Include(d => d.Vehicle)
                    .Include(d => d.DeliveryManLocation)
                    .Where(d => d.Vehicle != null && 
                               d.Vehicle.VehicleTypeId == request.Request.VehicleTypeId &&
                               d.Active == true &&
                               d.DeliveryManLocation != null)
                    .Select(d => new
                    {
                        d.Id,
                        d.FullName,
                        AndroidToken = d.AndriodDevice,
                        IosToken = d.IosDevice,
                        Latitude = d.DeliveryManLocation.Latitude,
                        Longitude = d.DeliveryManLocation.Longitude
                    })
                    .ToListAsync(cancellationToken);

                // Filter delivery men within 3km radius
                var deliveryMenWithinRadius = deliveryMenWithVehicle
                    .Where(d => CalculateDistance(originWaypoint.Latitude, originWaypoint.longitude, 
                                                  d.Latitude, d.Longitude) <= RadiusInKilometers)
                    .ToList();

                // Prepare notification
                var firebaseTokens = new List<string>();
                
                foreach (var deliveryMan in deliveryMenWithinRadius)
                {
                    if (!string.IsNullOrWhiteSpace(deliveryMan.AndroidToken))
                    {
                        firebaseTokens.Add(deliveryMan.AndroidToken);
                    }
                    if (!string.IsNullOrWhiteSpace(deliveryMan.IosToken))
                    {
                        firebaseTokens.Add(deliveryMan.IosToken);
                    }
                }

                // Send notifications if there are any tokens
                if (firebaseTokens.Any())
                {
                    var notificationBody = new NotificationBodyForMultipleDevices
                    {
                        Title = "New Order Available",
                        Body = $"New order #{order.OrderNumber} is available for pickup within your area",
                        FireBaseTokens = firebaseTokens,
                        PayLoad = new Dictionary<string, string>
                        {
                            { "orderId", order.Id.ToString() },
                            { "orderNumber", order.OrderNumber },
                            { "type", "new_order" }
                        }
                    };

                    await notificationService.SendNotificationAsyncToMultipleDevices(notificationBody);
                    
                    logger.LogInformation($"Notifications sent to {deliveryMenWithinRadius.Count} delivery men within {RadiusInKilometers}km for order {order.Id}");
                }

                var response = new SelectVehicleTypeResponseDto
                {
                    Success = true,
                    Message = $"Vehicle type selected successfully. Notified {deliveryMenWithinRadius.Count} delivery men within {RadiusInKilometers}km radius",
                    NotifiedDeliveryMenCount = deliveryMenWithinRadius.Count
                };

                return Result.Success(response);
            }

            public async Task<Result<GoogleResponse>> CalculateOrderDirection(Domain.Models.Order order)
            {
                var originWaypoint = order.OrderWayPoints
                                          .Where(wp => wp.IsOrgin)
                                          .Select(x => new LocationPoint { Latitude = x.Latitude, Longitude = x.longitude })
                                          .FirstOrDefault();

                if(originWaypoint is null)
                {
                    return Result.Failure<GoogleResponse>("Order must Have Orging");
                }

                var destinationWaypoint = order.OrderWayPoints
                                               .Where(wp => wp.IsDestination)
                                               .Select(x => new LocationPoint { Latitude = x.Latitude, Longitude = x.longitude })
                                               .FirstOrDefault();

                if (destinationWaypoint is null)
                {
                    return Result.Failure<GoogleResponse>("Order must Have Destentaion");
                }

                var wayPoints = order.OrderWayPoints
                    .Where(wp => !wp.IsOrgin && !wp.IsDestination)
                    .Select(x => new LocationPoint { Latitude = x.Latitude, Longitude = x.longitude })
                    .ToList();

                var response= await googleMapService.CalculateOrderDeliveryTime(originWaypoint, wayPoints, destinationWaypoint);
                return response;
            }


            private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
            {
                // Using Haversine formula to calculate distance between two points on Earth
                const double R = 6371; // Earth's radius in kilometers
                
                var dLat = ToRadians(lat2 - lat1);
                var dLon = ToRadians(lon2 - lon1);
                
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                        
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                
                return R * c; // Distance in kilometers
            }

            private static double ToRadians(double degrees)
            {
                return degrees * (Math.PI / 180);
            }
        }
    }
}