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
    public sealed record GetCustomerOrdersQuery(CustomerOrdersQueryRequest Request) : IRequest<Result<PagedCustomerOrdersDto>>
    {
        private class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery,
                                                                      Result<PagedCustomerOrdersDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetCustomerOrdersQueryHandler(INaqlahContext context,
                                                 IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<PagedCustomerOrdersDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                
                // Get customer ID from user session
                var customerId = await context.Customers
                    .Where(c => c.UserId == userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<PagedCustomerOrdersDto>("Customer not found");
                }

                // Build the query
                var query = context.Orders
                    .Include(o => o.OrderStatusHistories)
                    .Where(o => o.CustomerId == customerId);

                // Apply filters
                if (request.Request.StatusFilter.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == request.Request.StatusFilter.Value);
                }

                // Filter by date range using the first status history (creation date)
                if (request.Request.FromDate.HasValue)
                {
                    query = query.Where(o => o.OrderStatusHistories
                        .OrderBy(h => h.CreationDate)
                        .First().CreationDate >= request.Request.FromDate.Value);
                }

                if (request.Request.ToDate.HasValue)
                {
                    var toDateEndOfDay = request.Request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.OrderStatusHistories
                        .OrderBy(h => h.CreationDate)
                        .First().CreationDate <= toDateEndOfDay);
                }

                // Get total count before paging
                var totalCount = await query.CountAsync(cancellationToken);

                // Calculate pagination
                var pageNumber = request.Request.PageNumber < 1 ? 1 : request.Request.PageNumber;
                var pageSize = request.Request.PageSize < 1 ? 10 : request.Request.PageSize;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var skip = (pageNumber - 1) * pageSize;

                // Get paged data with left joins to DeliveryMan and VehicleType
                var ordersQuery = from o in query
                                  join dm in context.DeliveryMen on o.DeliveryManId equals dm.Id into deliveryManGroup
                                  from deliveryMan in deliveryManGroup.DefaultIfEmpty()
                                  join vt in context.VehicleTypes on o.VehicleTypeId equals vt.Id into vehicleTypeGroup
                                  from vehicleType in vehicleTypeGroup.DefaultIfEmpty()
                                  select new
                                  {
                                      Order = o,
                                      DeliveryMan = deliveryMan,
                                      VehicleType = vehicleType
                                  };

                var orders = await ordersQuery
                    .OrderByDescending(x => x.Order.OrderStatusHistories
                        .OrderBy(h => h.CreationDate)
                        .First().CreationDate)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new CustomerOrderListDto
                    {
                        Id = x.Order.Id,
                        OrderNumber = x.Order.OrderNumber,
                        CreatedDate = x.Order.OrderStatusHistories
                            .OrderBy(h => h.CreationDate)
                            .First().CreationDate,
                        Status = x.Order.OrderStatus,
                        StatusName = GetStatusName(x.Order.OrderStatus, languageId),
                        Total = x.Order.Total,
                       
                        DeliveryManName = x.DeliveryMan != null ? x.DeliveryMan.FullName : null
                    })
                    .ToListAsync(cancellationToken);

                var result = new PagedCustomerOrdersDto
                {
                    Orders = orders,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };

                return Result.Success(result);
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
        }
    }
}