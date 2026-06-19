using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed class CaptainControlOrdersRequest
    {
        public IReadOnlyList<int>? DeliveryManIds { get; init; }
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public OrderStatus? StatusFilter { get; init; }
        public bool? ActiveOrdersOnly { get; init; }
        public CustomerType? CustomerTypeFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;
    }

    public sealed class CaptainControlOrdersService
    {
        private readonly INaqlahContext _context;

        public CaptainControlOrdersService(INaqlahContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<GetAllOrdersDto>>> GetAsync(
            CaptainControlOrdersRequest request,
            CancellationToken cancellationToken)
        {
            var deliveryManIds = request.DeliveryManIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (deliveryManIds is { Count: > 0 })
            {
                var validCount = await _context.DeliveryMen
                    .CountAsync(
                        dm => deliveryManIds.Contains(dm.Id) && dm.DeliveryState == DeliveryRequesState.Approved,
                        cancellationToken);

                if (validCount != deliveryManIds.Count)
                    return Result.Failure<PagedResult<GetAllOrdersDto>>("DeliveryManNotFound");
            }

            var isArabic = request.LanguageId == (int)Language.Arabic;
            var searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();

            var query = from order in _context.Orders
                        join customer in _context.Customers on order.CustomerId equals customer.Id
                        join customerUser in _context.Users on customer.UserId equals customerUser.Id
                        join deliveryMan in _context.DeliveryMen on order.DeliveryManId equals deliveryMan.Id into deliveryManGroup
                        from deliveryMan in deliveryManGroup.DefaultIfEmpty()
                        join deliveryManUser in _context.Users on deliveryMan.UserId equals deliveryManUser.Id into deliveryManUserGroup
                        from deliveryManUser in deliveryManUserGroup.DefaultIfEmpty()
                        where order.DeliveryManId != null
                        select new
                        {
                            Order = order,
                            CustomerName = customerUser.UserName ?? "غير محدد",
                            CustomerPhone = customerUser.PhoneNumber ?? "غير محدد",
                            CustomerType = customer.CustomerType,
                            DeliveryManId = order.DeliveryManId,
                            DeliveryManName = deliveryManUser != null ? deliveryManUser.UserName ?? "غير محدد" : "غير محدد",
                            DeliveryManPhone = deliveryManUser != null ? deliveryManUser.PhoneNumber ?? "غير محدد" : "غير محدد"
                        };

            if (deliveryManIds is { Count: > 0 })
                query = query.Where(x => x.DeliveryManId.HasValue && deliveryManIds.Contains(x.DeliveryManId.Value));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Order.OrderNumber.ToLower().Contains(searchLower) ||
                    x.CustomerName.ToLower().Contains(searchLower) ||
                    x.CustomerPhone.Contains(searchLower) ||
                    x.DeliveryManName.ToLower().Contains(searchLower) ||
                    x.DeliveryManPhone.Contains(searchLower));
            }

            if (request.ActiveOrdersOnly == true)
            {
                var activeStatuses = new[]
                {
                    OrderStatus.Assigned,
                    OrderStatus.ConfirmedGoingToPickup,
                    OrderStatus.PickedUpFromDeliveryMan
                };
                query = query.Where(x => activeStatuses.Contains(x.Order.OrderStatus));
            }
            else if (request.StatusFilter.HasValue)
            {
                query = query.Where(x => x.Order.OrderStatus == request.StatusFilter.Value);
            }

            if (request.CustomerTypeFilter.HasValue)
                query = query.Where(x => x.CustomerType == request.CustomerTypeFilter.Value);

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value.Date.ToUniversalTime();
                query = query.Where(x => x.Order.CreationDate >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(x => x.Order.CreationDate <= toDate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageRows = await query
                .OrderByDescending(x => x.Order.CreationDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            var orderIds = pageRows.Select(x => x.Order.Id).ToList();

            var ordersWithWaypoints = await _context.Orders
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.City)
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.Neighborhood)
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.Region)
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync(cancellationToken);

            var ordersDict = ordersWithWaypoints.ToDictionary(o => o.Id);
            var rowDict = pageRows.ToDictionary(x => x.Order.Id);

            var items = orderIds
                .Where(id => ordersDict.ContainsKey(id) && rowDict.ContainsKey(id))
                .Select(id =>
                {
                    var order = ordersDict[id];
                    var row = rowDict[id];

                    return new GetAllOrdersDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        CreatedDate = order.CreationDate,
                        Status = order.OrderStatus,
                        StatusName = OrderDisplayLabels.GetOrderStatusName(order.OrderStatus, request.LanguageId),
                        OrderType = order.OrderType,
                        OrderTypeName = OrderDisplayLabels.GetOrderTypeName(order.OrderType, request.LanguageId),
                        Total = order.Total,
                        CustomerId = order.CustomerId,
                        CustomerName = row.CustomerName,
                        CustomerPhone = row.CustomerPhone,
                        CustomerType = row.CustomerType,
                        CustomerTypeName = OrderDisplayLabels.GetCustomerTypeName(row.CustomerType, request.LanguageId),
                        DeliveryManId = row.DeliveryManId,
                        DeliveryManName = row.DeliveryManName,
                        DeliveryManPhone = row.DeliveryManPhone,
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
                            StatusName = OrderDisplayLabels.GetWayPointStatusName(wp.OrderWayPointsStatus, request.LanguageId),
                            PickedUpDate = wp.PickedUpDate
                        }).ToList()
                    };
                })
                .ToList();

            var totalPages = request.Take > 0 ? (int)Math.Ceiling((double)totalCount / request.Take) : 0;

            return Result.Success(new PagedResult<GetAllOrdersDto>
            {
                Data = items,
                TotalCount = totalCount,
                TotalPages = totalPages
            });
        }

        public async Task<Result<List<GetAllOrdersDto>>> GetAllForExportAsync(
            CaptainControlOrdersRequest request,
            int maxRows,
            CancellationToken cancellationToken)
        {
            var exportRequest = new CaptainControlOrdersRequest
            {
                DeliveryManIds = request.DeliveryManIds,
                Skip = 0,
                Take = maxRows,
                SearchTerm = request.SearchTerm,
                StatusFilter = request.StatusFilter,
                ActiveOrdersOnly = request.ActiveOrdersOnly,
                CustomerTypeFilter = request.CustomerTypeFilter,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                LanguageId = request.LanguageId
            };

            var result = await GetAsync(exportRequest, cancellationToken);
            if (result.IsFailure)
                return Result.Failure<List<GetAllOrdersDto>>(result.Error);

            return Result.Success(result.Value.Data);
        }
    }
}
