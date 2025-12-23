using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
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
    public class GetAllOrdersQuery : IRequest<Result<PagedResult<GetAllOrdersDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public OrderStatus? StatusFilter { get; init; }
        public CustomerType? CustomerTypeFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<GetAllOrdersDto>>>
        {
            private readonly INaqlahContext context;

            public GetAllOrdersQueryHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<PagedResult<GetAllOrdersDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    // Build simple query - no includes, simple joins
                    var query = from order in context.Orders
                                join customer in context.Customers on order.CustomerId equals customer.Id
                                join customerUser in context.Users on customer.UserId equals customerUser.Id
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

                    if (request.CustomerTypeFilter.HasValue)
                    {
                        query = query.Where(x => x.CustomerType == request.CustomerTypeFilter.Value);
                    }

                    if (request.FromDate.HasValue)
                    {
                        // Use Id as proxy for creation order since CreatedDate doesn't exist
                        query = query.Where(x => x.Order.Id >= request.FromDate.Value.Day);
                    }

                    if (request.ToDate.HasValue)
                    {
                        // Use Id as proxy for creation order since CreatedDate doesn't exist
                        query = query.Where(x => x.Order.Id <= request.ToDate.Value.Day * 1000);
                    }

                    // Get total count
                    var totalCount = await query.CountAsync(cancellationToken);

                    // Apply pagination and get order data with waypoints
                    var orderIds = await query
                        .OrderByDescending(x => x.Order.Id)
                        .Skip(request.Skip)
                        .Take(request.Take)
                        .Select(x => x.Order.Id)
                        .ToListAsync(cancellationToken);

                    // Get orders with their waypoints
                    var ordersWithWaypoints = await (from order in context.Orders
                                                     join customer in context.Customers on order.CustomerId equals customer.Id
                                                     join customerUser in context.Users on customer.UserId equals customerUser.Id
                                                     where orderIds.Contains(order.Id)
                                                     select new
                                                     {
                                                         Order = order,
                                                         CustomerName = customerUser.UserName,
                                                         CustomerPhone = customerUser.PhoneNumber,
                                                         CustomerType = customer.CustomerType,
                                                         WayPoints = order.OrderWayPoints
                                                             .Select(wp => new OrderWayPointAdminDto
                                                             {
                                                                 Id = wp.Id,
                                                                 Latitude = wp.Latitude,
                                                                 Longitude = wp.longitude,
                                                                 IsOrigin = wp.IsOrgin,
                                                                 IsDestination = wp.IsDestination,
                                                                 Status = wp.OrderWayPointsStatus,
                                                                 StatusName = GetWayPointStatusName(wp.OrderWayPointsStatus, request.LanguageId),
                                                                 PickedUpDate = wp.PickedUpDate,
                                                                 NeighborhoodName = request.LanguageId == 1 ? wp.Neighborhood.ArabicName ?? string.Empty : wp.Neighborhood.EnglishName ?? string.Empty,
                                                                 Address = request.LanguageId == 1 ?
                                                                           wp.City.ArabicName + "-" + wp.Region.ArabicName : wp.City.EnglishName + "-" + wp.Region.EnglishName,
                                                             }).ToList()
                                                     })
                                                   .ToListAsync(cancellationToken);

                    var items = ordersWithWaypoints.Select(x => new GetAllOrdersDto
                    {
                        Id = x.Order.Id,
                        OrderNumber = x.Order.OrderNumber,
                        Status = x.Order.OrderStatus,
                        StatusName = GetOrderStatusName(x.Order.OrderStatus, request.LanguageId),
                        OrderType = x.Order.OrderType,
                        OrderTypeName = GetOrderTypeName(x.Order.OrderType, request.LanguageId),
                        Total = x.Order.Total,
                        CustomerId = x.Order.CustomerId,
                        CustomerName = x.CustomerName,
                        CustomerPhone = x.CustomerPhone,
                        CustomerType = x.CustomerType,
                        CustomerTypeName = GetCustomerTypeName(x.CustomerType, request.LanguageId),
                        WayPoints = x.WayPoints
                    }).ToList();

                    var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                    var pagedResult = new PagedResult<GetAllOrdersDto>
                    {
                        Data = items,
                        TotalCount = totalCount,
                        TotalPages = totalPages
                    };

                    return Result.Success(pagedResult);
                }
                catch (Exception ex)
                {
                    return Result.Failure<PagedResult<GetAllOrdersDto>>($"Failed to get orders: {ex.Message}");
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
        }
    }
}