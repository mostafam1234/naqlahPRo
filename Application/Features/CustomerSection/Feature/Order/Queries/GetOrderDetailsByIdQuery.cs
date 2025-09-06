using Application.Features.CustomerSection.Feature.Order.Dtos;
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

namespace Application.Features.CustomerSection.Feature.Order.Queries
{
    public sealed record GetOrderDetailsByIdQuery(int OrderId) : IRequest<Result<OrderDetailsDto>>
    {
        private class GetOrderDetailsByIdQueryHandler : IRequestHandler<GetOrderDetailsByIdQuery,
                                                                        Result<OrderDetailsDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetOrderDetailsByIdQueryHandler(INaqlahContext context,
                                                   IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<OrderDetailsDto>> Handle(GetOrderDetailsByIdQuery request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                
                // Get customer ID to verify ownership
                var customerId = await context.Customers
                    .Where(c => c.UserId == userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<OrderDetailsDto>("Customer not found");
                }

                // Get the order with all related data
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.OrderServices)
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.Region)
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.City)
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.Neighborhood)
                    .Include(o => o.PaymentMethods)
                    .Include(o => o.OrderStatusHistories)
                    .Include(o => o.OrderPackage)
                    .Where(o => o.Id == request.OrderId && o.CustomerId == customerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (order == null)
                {
                    return Result.Failure<OrderDetailsDto>("Order not found or you don't have permission to view it");
                }

                // Get DeliveryMan details if assigned
                Domain.Models.DeliveryMan deliveryMan = null;
                if (order.DeliveryManId.HasValue)
                {
                    deliveryMan = await context.DeliveryMen
                        .Where(dm => dm.Id == order.DeliveryManId.Value)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                // Get VehicleType details if selected
                Domain.Models.VehicleType vehicleType = null;
                if (order.VehicleTypeId.HasValue)
                {
                    vehicleType = await context.VehicleTypes
                        .Where(vt => vt.Id == order.VehicleTypeId.Value)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                // Map to DTO
                var orderDetailsDto = new OrderDetailsDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedDate = order.OrderStatusHistories
                        .OrderBy(h => h.CreationDate)
                        .First().CreationDate,
                    Status = order.OrderStatus,
                    StatusName = GetStatusName(order.OrderStatus, languageId),
                    Total = order.Total,
                    OrderType = order.OrderType,
                    OrderTypeName = GetOrderTypeName(order.OrderType, languageId),
                    VehicleTypeId = order.VehicleTypeId,
                    VehicleTypeName = vehicleType != null ? 
                        (languageId == (int)Language.Arabic ? vehicleType.ArabicName : vehicleType.EnglishName) : null,
                    DeliveryManId = order.DeliveryManId,
                    DeliveryManName = deliveryMan?.FullName,
                    DeliveryManPhone = deliveryMan?.PhoneNumber,
                    PackageName = order.OrderPackage != null ?
                        (languageId == (int)Language.Arabic ? order.OrderPackage.ArabicDescripton : order.OrderPackage.EnglishDescription) : null,
                    Categories = order.OrderDetails.Select(od => new OrderCategoryDto
                    {
                        CategoryId = od.MainCategoryId,
                        CategoryName = languageId == (int)Language.Arabic ? 
                            od.ArabicCategoryName : od.EnglishCategoryName
                    }).ToList(),
                    Services = order.OrderServices.Select(os => new OrderServiceDto
                    {
                        ServiceId = os.Id,
                        ServiceName = languageId == (int)Language.Arabic ?
                            os.ArabicName : os.EnglishName,
                        Amount = os.Amount
                    }).ToList(),
                    WayPoints = order.OrderWayPoints.Select(wp => new OrderWayPointDto
                    {
                        Id = wp.Id,
                        Latitude = wp.Latitude,
                        Longitude = wp.longitude,
                        IsOrigin = wp.IsOrgin,
                        IsDestination = wp.IsDestination,
                        Status = wp.OrderWayPointsStatus,
                        StatusName = GetWayPointStatusName(wp.OrderWayPointsStatus, languageId),
                        PickedUpDate = wp.PickedUpDate,
                        RegionName = wp.Region != null ?
                            (languageId == (int)Language.Arabic ? wp.Region.ArabicName : wp.Region.EnglishName) : null,
                        CityName = wp.City != null ?
                            (languageId == (int)Language.Arabic ? wp.City.ArabicName : wp.City.EnglishName) : null,
                        NeighborhoodName = wp.Neighborhood != null ?
                            (languageId == (int)Language.Arabic ? wp.Neighborhood.ArabicName : wp.Neighborhood.EnglishName) : null,
                        Address = BuildAddress(wp, languageId)
                    }).ToList(),
                    PaymentMethods = order.PaymentMethods.Select(pm => new OrderPaymentMethodDto
                    {
                        PaymentMethodId = pm.PaymentMethodId,
                        PaymentMethodName = GetPaymentMethodName(pm.PaymentMethodId, languageId),
                        Amount = pm.Amount
                    }).ToList()
                };

                return Result.Success(orderDetailsDto);
            }

            private string BuildAddress(Domain.Models.OrderWayPoint waypoint, int languageId)
            {
                var parts = new List<string>();
                
                if (waypoint.Neighborhood != null)
                {
                    parts.Add(languageId == (int)Language.Arabic ? 
                        waypoint.Neighborhood.ArabicName : waypoint.Neighborhood.EnglishName);
                }
                
                if (waypoint.City != null)
                {
                    parts.Add(languageId == (int)Language.Arabic ? 
                        waypoint.City.ArabicName : waypoint.City.EnglishName);
                }
                
                if (waypoint.Region != null)
                {
                    parts.Add(languageId == (int)Language.Arabic ? 
                        waypoint.Region.ArabicName : waypoint.Region.EnglishName);
                }
                
                return string.Join(", ", parts);
            }

            private string GetStatusName(OrderStatus status, int languageId)
            {
                if (languageId == (int)Language.Arabic)
                {
                    return status switch
                    {
                        OrderStatus.Pending => "قيد الانتظار",
                        OrderStatus.Assigned => "تم التعيين",
                        OrderStatus.Cancelled => "ملغي",
                        OrderStatus.Completed => "مكتمل",
                        _ => status.ToString()
                    };
                }

                return status.ToString();
            }

            private string GetOrderTypeName(OrderType orderType, int languageId)
            {
                if (languageId == (int)Language.Arabic)
                {
                    return orderType switch
                    {
                        OrderType.SingleWayPoints => "نقطة واحدة",
                        OrderType.MultiWayPoints => "نقاط متعددة",
                        OrderType.BackAndForth => "ذهاب وإياب",
                        _ => orderType.ToString()
                    };
                }

                return orderType.ToString();
            }

            private string GetWayPointStatusName(OrderWayPointsStatus status, int languageId)
            {
                if (languageId == (int)Language.Arabic)
                {
                    return status switch
                    {
                        OrderWayPointsStatus.Pending => "قيد الانتظار",
                        OrderWayPointsStatus.PickedUp => "تم الاستلام",
                        OrderWayPointsStatus.Completed => "تم التسليم",
                        _ => status.ToString()
                    };
                }

                return status.ToString();
            }

            private string GetPaymentMethodName(int paymentMethodId, int languageId)
            {
                // Map payment method ID to name based on PaymentMethodEnum
                var paymentMethod = (PaymentMethodEnum)paymentMethodId;
                
                if (languageId == (int)Language.Arabic)
                {
                    return paymentMethod switch
                    {
                        PaymentMethodEnum.Cash => "نقدي",
                        PaymentMethodEnum.Online => "اونلاين",
                        PaymentMethodEnum.Wallet => "المحفظة",
                       
                    };
                }

                return paymentMethod.ToString();
            }
        }
    }
}