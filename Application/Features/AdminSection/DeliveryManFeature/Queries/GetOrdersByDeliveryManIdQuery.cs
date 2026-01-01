using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetOrdersByDeliveryManIdQuery : IRequest<Result<PagedResult<GetAllOrdersDto>>>
    {
        public int DeliveryManId { get; init; }
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public OrderStatus? StatusFilter { get; init; }
        public int LanguageId { get; init; } = 1;

        private class GetOrdersByDeliveryManIdQueryHandler : IRequestHandler<GetOrdersByDeliveryManIdQuery, Result<PagedResult<GetAllOrdersDto>>>
        {
            private readonly INaqlahContext _context;

            public GetOrdersByDeliveryManIdQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<GetAllOrdersDto>>> Handle(GetOrdersByDeliveryManIdQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                // Check if delivery man exists
                var deliveryManExists = await _context.DeliveryMen
                    .AnyAsync(dm => dm.Id == request.DeliveryManId && dm.DeliveryState == DeliveryRequesState.Approved, cancellationToken);

                if (!deliveryManExists)
                {
                    var errorMessage = request.LanguageId == 1 ? "مندوب التوصيل غير موجود." : "Delivery man not found.";
                    return Result.Failure<PagedResult<GetAllOrdersDto>>(errorMessage);
                }

                var query = from order in _context.Orders
                           join customer in _context.Customers on order.CustomerId equals customer.Id
                           join customerUser in _context.Users on customer.UserId equals customerUser.Id
                           where order.DeliveryManId == request.DeliveryManId
                           select new
                           {
                               Order = order,
                               CustomerName = customerUser.UserName ?? "غير محدد",
                               CustomerPhone = customerUser.PhoneNumber ?? "غير محدد",
                               CustomerType = customer.CustomerType
                           };

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.Order.OrderNumber.ToLower().Contains(searchLower) ||
                        x.CustomerName.ToLower().Contains(searchLower) ||
                        x.CustomerPhone.Contains(searchLower)
                    );
                }

                if (request.StatusFilter.HasValue)
                {
                    query = query.Where(x => x.Order.OrderStatus == request.StatusFilter.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and get order data with waypoints
                var orderIds = await query
                    .OrderByDescending(x => x.Order.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => x.Order.Id)
                    .ToListAsync(cancellationToken);

                // Get orders with waypoints using Include (same pattern as GetAllOrdersQuery)
                var ordersWithWaypoints = await _context.Orders
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.City)
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.Neighborhood)
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(wp => wp.Region)
                    .Where(o => orderIds.Contains(o.Id) && o.DeliveryManId == request.DeliveryManId)
                    .ToListAsync(cancellationToken);

                var customers = await (from customer in _context.Customers
                                      join customerUser in _context.Users on customer.UserId equals customerUser.Id
                                      where ordersWithWaypoints.Select(o => o.CustomerId).Contains(customer.Id)
                                      select new
                                      {
                                          CustomerId = customer.Id,
                                          CustomerName = customerUser.UserName ?? "غير محدد",
                                          CustomerPhone = customerUser.PhoneNumber ?? "غير محدد",
                                          CustomerType = customer.CustomerType
                                      }).ToListAsync(cancellationToken);

                var customersDict = customers.ToDictionary(c => c.CustomerId);

                var items = ordersWithWaypoints.Select(order =>
                {
                    var customer = customersDict.ContainsKey(order.CustomerId) ? customersDict[order.CustomerId] : null;

                    return new GetAllOrdersDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        CustomerName = customer?.CustomerName ?? "غير محدد",
                        CustomerPhone = customer?.CustomerPhone ?? "غير محدد",
                        CustomerType = customer?.CustomerType ?? CustomerType.Individual,
                        Status = order.OrderStatus,
                        StatusName = GetOrderStatusName(order.OrderStatus, request.LanguageId),
                        Total = order.Total,
                        WayPoints = order.OrderWayPoints.Select(wp => new OrderWayPointAdminDto
                        {
                            Id = wp.Id,
                            Latitude = wp.Latitude,
                            Longitude = wp.longitude,
                            IsOrigin = wp.IsOrgin,
                            IsDestination = wp.IsDestination,
                            Address = (isArabic ? wp.City.ArabicName : wp.City.EnglishName) +
                                     (wp.Neighborhood != null ? " - " + (isArabic ? wp.Neighborhood.ArabicName : wp.Neighborhood.EnglishName) : ""),
                            CityName = isArabic ? wp.City.ArabicName : wp.City.EnglishName,
                            NeighborhoodName = wp.Neighborhood != null ? (isArabic ? wp.Neighborhood.ArabicName : wp.Neighborhood.EnglishName) : string.Empty,
                            RegionName = wp.Region != null ? (isArabic ? wp.Region.ArabicName : wp.Region.EnglishName) : string.Empty,
                            Status = wp.OrderWayPointsStatus,
                            StatusName = GetWayPointStatusName(wp.OrderWayPointsStatus, request.LanguageId),
                            PickedUpDate = wp.PickedUpDate
                        }).ToList()
                    };
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var result = new PagedResult<GetAllOrdersDto>
                {
                    Data = items,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(result);
            }

            private static string GetOrderStatusName(OrderStatus status, int languageId)
            {
                return status switch
                {
                    OrderStatus.Pending => languageId == 1 ? "معلق" : "Pending",
                    OrderStatus.Assigned => languageId == 1 ? "مخصص" : "Assigned",
                    OrderStatus.Completed => languageId == 1 ? "مكتمل" : "Completed",
                    OrderStatus.Cancelled => languageId == 1 ? "ملغي" : "Cancelled",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }

            private static string GetWayPointStatusName(OrderWayPointsStatus status, int languageId)
            {
                return status switch
                {
                    OrderWayPointsStatus.Pending => languageId == 1 ? "في الانتظار" : "Pending",
                    OrderWayPointsStatus.PickedUp => languageId == 1 ? "تم الوصول الى نقطة الإستلام" : "Picked Up",
                    OrderWayPointsStatus.Completed => languageId == 1 ? "مكتمل" : "Completed",
                    _ => languageId == 1 ? "غير محدد" : "Not Specified"
                };
            }
        }
    }
}
