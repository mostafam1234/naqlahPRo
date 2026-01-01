using Application.Features.AdminSection.OrderFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public class GetOrderDetailsByOrderIdForAdminQuery : IRequest<Result<GetOrderDetailsForAdminDto>>
    {
        public int OrderId { get; init; }
        public int LanguageId { get; init; } = 1; // Default to Arabic

        private class GetOrderDetailsByOrderIdForAdminQueryHandler : IRequestHandler<GetOrderDetailsByOrderIdForAdminQuery, Result<GetOrderDetailsForAdminDto>>
        {
            private readonly INaqlahContext context;

            public GetOrderDetailsByOrderIdForAdminQueryHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<GetOrderDetailsForAdminDto>> Handle(GetOrderDetailsByOrderIdForAdminQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    // Get basic order info with minimal includes
                    var order = await context.Orders
                        .Include(o => o.OrderPackage)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result.Failure<GetOrderDetailsForAdminDto>("Order not found");
                    }

                    // Get customer info separately
                    var customer = await (from c in context.Customers
                                          join u in context.Users on c.UserId equals u.Id
                                          where c.Id == order.CustomerId
                                          select new { Customer = c, User = u })
                                        .FirstOrDefaultAsync(cancellationToken);

                    // Get order categories with separate query - Fix property names
                    var orderCategories = await (from detail in context.Orders
                                               .Where(o => o.Id == request.OrderId)
                                               .SelectMany(o => o.OrderDetails)
                                                 join category in context.MainCategories on detail.MainCategoryId equals category.Id
                                                 select new OrderCategoryAdminDto
                                                 {
                                                     Id = detail.Id,
                                                     CategoryId = detail.MainCategoryId,
                                                     CategoryName = category.ArabicName,
                                                     Quantity = 1, // Default value since not in model
                                                     Price = 0, // Default value since not in model
                                                     TotalPrice = 0 // Default value since not in model
                                                 }).ToListAsync(cancellationToken);

                    // Get order services with separate query
                    var orderServices = await (from service in context.Orders
                                             .Where(o => o.Id == request.OrderId)
                                             .SelectMany(o => o.OrderServices)
                                               join work in context.AssistanWorks on service.WorkId equals work.Id
                                               select new OrderServiceAdminDto
                                               {
                                                   Id = service.Id,
                                                   ServiceId = service.WorkId,
                                                   ServiceName = work.ArabicName,
                                                   Price = service.Amount // Use Amount instead of Price
                                               }).ToListAsync(cancellationToken);

                    // Get waypoints with separate query - without City/Neighborhood joins since not in context
                    var wayPoints = await (from wp in context.Orders
                                         .Where(o => o.Id == request.OrderId)
                                         .SelectMany(o => o.OrderWayPoints)
                                           join region in context.Regions on wp.RegionId equals region.Id
                                           select new OrderWayPointAdminDto
                                           {
                                               Id = wp.Id,
                                               Latitude = wp.Latitude,
                                               Longitude = wp.longitude,
                                               IsOrigin = wp.IsOrgin,
                                               IsDestination = wp.IsDestination,
                                               Status = wp.OrderWayPointsStatus,
                                               StatusName = GetWayPointStatusName(wp.OrderWayPointsStatus, request.LanguageId),
                                               PickedUpDate = wp.PickedUpDate,
                                               CityName = request.LanguageId == 1 ? wp.City.ArabicName ?? string.Empty : wp.City.EnglishName ?? string.Empty,
                                               RegionName = request.LanguageId == 1 ? wp.Region.ArabicName ?? string.Empty : wp.Region.EnglishName ?? string.Empty,
                                               NeighborhoodName = request.LanguageId == 1 ? wp.Neighborhood.ArabicName ?? string.Empty : wp.Neighborhood.EnglishName ?? string.Empty,
                                               Address = request.LanguageId == 1 ?
                                                        (wp.City.ArabicName ?? string.Empty) + " - " + (wp.Neighborhood.ArabicName ?? string.Empty) : 
                                                        (wp.City.EnglishName ?? string.Empty) + " - " + (wp.Neighborhood.EnglishName ?? string.Empty),
                                           }).ToListAsync(cancellationToken);

                    // Get payment methods with separate query
                    var paymentMethods = await context.Orders
                        .Where(o => o.Id == request.OrderId)
                        .SelectMany(o => o.PaymentMethods)
                        .Select(pm => new OrderPaymentMethodAdminDto
                        {
                            PaymentMethodId = pm.PaymentMethodId,
                            PaymentMethodName = pm.PaymentMethod.ArabicName,
                            Amount = pm.Amount,
                            PaymentStatus = pm.OrderPaymentStatus,
                            PaymentStatusName = GetPaymentStatusName(pm.OrderPaymentStatus, request.LanguageId)
                        })
                        .ToListAsync(cancellationToken);

                    // Get status history with separate query
                    var statusHistory = await context.Orders
                        .Where(o => o.Id == request.OrderId)
                        .SelectMany(o => o.OrderStatusHistories)
                        .OrderBy(h => h.CreationDate)
                        .Select(h => new OrderStatusHistoryDto
                        {
                            Status = h.OrderStatus,
                            StatusName = GetOrderStatusName(h.OrderStatus, request.LanguageId),
                            CreationDate = h.CreationDate,
                            Description = GetStatusDescription(h.OrderStatus, request.LanguageId)
                        })
                        .ToListAsync(cancellationToken);

                    // Get assigned and completed dates
                    var assignedDate = statusHistory.FirstOrDefault(h => h.Status == OrderStatus.Assigned)?.CreationDate;
                    var completedDate = statusHistory.FirstOrDefault(h => h.Status == OrderStatus.Completed)?.CreationDate;

                    // Get DeliveryMan info if assigned
                    var deliveryMan = order.DeliveryManId.HasValue ?
                        await (from dm in context.DeliveryMen
                               join u in context.Users on dm.UserId equals u.Id
                               where dm.Id == order.DeliveryManId.Value
                               select new { DeliveryMan = dm, User = u })
                               .FirstOrDefaultAsync(cancellationToken) : null;

                    // Get VehicleType if assigned
                    var vehicleType = order.VehicleTypeId.HasValue ?
                        await context.VehicleTypes.FirstOrDefaultAsync(vt => vt.Id == order.VehicleTypeId.Value, cancellationToken) : null;

                    // Build the response DTO
                    var orderDetails = new GetOrderDetailsForAdminDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.OrderStatus,
                        StatusName = GetOrderStatusName(order.OrderStatus, request.LanguageId),
                        OrderType = order.OrderType,
                        OrderTypeName = GetOrderTypeName(order.OrderType, request.LanguageId),
                        Total = order.Total,

                        // Customer Information
                        CustomerId = order.CustomerId,
                        CustomerName = customer?.User.UserName ?? "غير محدد",
                        CustomerPhone = customer?.User.PhoneNumber ?? "غير محدد",
                        CustomerType = customer?.Customer.CustomerType ?? CustomerType.Individual,
                        CustomerTypeName = customer != null ? GetCustomerTypeName(customer.Customer.CustomerType, request.LanguageId) : "غير محدد",

                        // Delivery Man Information
                        DeliveryManId = order.DeliveryManId,
                        DeliveryManName = deliveryMan?.User.UserName
                                        ?? (request.LanguageId == 1 ? "غير مُعين" : "Unassigned"),
                        DeliveryManPhone = deliveryMan?.User.PhoneNumber ??
                                            (request.LanguageId == 1 ? "غير محدد" : "Not Specified"),

                        // Vehicle Information
                        VehicleTypeId = order.VehicleTypeId,
                        VehicleTypeName = request.LanguageId == 1 ? vehicleType?.ArabicName ?? "غير محدد" : vehicleType?.EnglishName ?? "Not Specified",

                        // Package Information
                        OrderPackage = new OrderPackageDto
                        {
                            Id = order.OrderPackage.Id,
                            Description = request.LanguageId == 1 ? order.OrderPackage.ArabicDescripton : order.OrderPackage.EnglishDescription,
                            MinWeightInKg = order.OrderPackage.MinWeightInKiloGram,
                            MaxWeightInKg = order.OrderPackage.MaxWeightInKiloGram
                        },

                        // Related data
                        WayPoints = wayPoints,
                        OrderCategories = orderCategories,
                        OrderServices = orderServices,
                        OrderPaymentMethods = paymentMethods,
                        StatusHistory = statusHistory,
                        AssignedDate = assignedDate,
                        CompletedDate = completedDate
                    };

                    return Result.Success(orderDetails);
                }
                catch (Exception ex)
                {
                    return Result.Failure<GetOrderDetailsForAdminDto>($"Failed to get order details: {ex.Message}");
                }
            }

            private static string GetOrderStatusName(OrderStatus status, int languageId)
            {
                return status switch
                {
                    OrderStatus.Pending => languageId == 1 ? "في الانتظار" : "Pending",
                    OrderStatus.Assigned => languageId == 1 ? "تم تعيين كابتن" : "Assigned",
                    OrderStatus.Completed => languageId == 1 ? "مكتمل" : "Completed",
                    OrderStatus.Cancelled => languageId == 1 ? "ملغي" : "Cancelled",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetOrderTypeName(OrderType orderType, int languageId)
            {
                return orderType switch
                {
                    OrderType.SingleWayPoints => languageId == 1 ? "نقطة واحدة" : "Single Way Point",
                    OrderType.MultiWayPoints => languageId == 1 ? "عدة نقاط" : "Multiple Way Points",
                    OrderType.BackAndForth => languageId == 1 ? "ذهاب وعودة" : "Back and Forth",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetCustomerTypeName(CustomerType customerType, int languageId)
            {
                return customerType switch
                {
                    CustomerType.Individual => languageId == 1 ? "فرد" : "Individual",
                    CustomerType.Establishment => languageId == 1 ? "مؤسسة" : "Establishment",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetWayPointStatusName(OrderWayPointsStatus status, int languageId)
            {
                return status switch
                {
                    OrderWayPointsStatus.Pending => languageId == 1 ? "في الانتظار" : "Pending  ",
                    OrderWayPointsStatus.PickedUp => languageId == 1 ? "تم الوصول الى نقطة الإستلام" : "Picked Up",
                    OrderWayPointsStatus.Completed => languageId == 1 ? "مكتمل" : "Completed",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetPaymentStatusName(OrderPaymentStatus status, int languageId)
            {
                return status switch
                {
                    OrderPaymentStatus.Pending => languageId == 1 ? "قيد الانتظار" : "Pending",
                    OrderPaymentStatus.Captured => languageId == 1 ? "مدفوع" : "Captured",
                    OrderPaymentStatus.Failed => languageId == 1 ? "فشل" : "Failed",
                    OrderPaymentStatus.Cancelled => languageId == 1 ? "ملغي" : "Cancelled",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetStatusDescription(OrderStatus status, int languageId)
            {
                return status switch
                {
                    OrderStatus.Pending => languageId == 1 ? "تم إنشاء الطلب وهو في انتظار التعيين" : "Order created and awaiting assignment",
                    OrderStatus.Assigned => languageId == 1 ? "تم تعيين الطلب لمندوب التوصيل" : "Order assigned to delivery agent",
                    OrderStatus.Completed => languageId == 1 ? "تم إكمال الطلب بنجاح" : "Order completed successfully",
                    OrderStatus.Cancelled => languageId == 1 ? "تم إلغاء الطلب" : "Order cancelled",
                    _ => languageId == 1 ? "حالة غير معروفة" : "Unknown status"
                };
            }
        }
    }
}