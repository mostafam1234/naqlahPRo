using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public sealed record GetOrderStatisticsQuery : IRequest<Result<OrderStatisticsDto>>
    {
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<OrderStatisticsDto>> Handle(
                GetOrderStatisticsQuery request,
                CancellationToken cancellationToken)
            {
                var statusCounts = await _context.Orders
                    .AsNoTracking()
                    .GroupBy(o => o.OrderStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var countLookup = statusCounts.ToDictionary(x => x.Status, x => x.Count);
                int Count(OrderStatus status) => countLookup.TryGetValue(status, out var count) ? count : 0;

                var assigned = Count(OrderStatus.Assigned);
                var confirmed = Count(OrderStatus.ConfirmedGoingToPickup);
                var pickedUp = Count(OrderStatus.PickedUpFromDeliveryMan);
                var completed = Count(OrderStatus.Completed);
                var cancelled = Count(OrderStatus.Cancelled);

                var ordersByStatus = new List<OrderStatusCountDto>
                {
                    BuildStatusCount(OrderStatus.Pending, Count(OrderStatus.Pending), request.LanguageId),
                    BuildStatusCount(OrderStatus.Assigned, assigned, request.LanguageId),
                    BuildStatusCount(OrderStatus.ConfirmedGoingToPickup, confirmed, request.LanguageId),
                    BuildStatusCount(OrderStatus.PickedUpFromDeliveryMan, pickedUp, request.LanguageId),
                    BuildStatusCount(OrderStatus.Completed, completed, request.LanguageId),
                    BuildStatusCount(OrderStatus.Cancelled, cancelled, request.LanguageId)
                };

                return Result.Success(new OrderStatisticsDto
                {
                    TotalOrders = statusCounts.Sum(x => x.Count),
                    ActiveOrders = assigned + confirmed + pickedUp,
                    ConfirmedGoingToPickupOrders = confirmed,
                    PickedUpOrders = pickedUp,
                    CompletedOrders = completed,
                    CancelledOrders = cancelled,
                    OrdersByStatus = ordersByStatus
                });
            }

            private static OrderStatusCountDto BuildStatusCount(
                OrderStatus status,
                int count,
                int languageId)
            {
                return new OrderStatusCountDto
                {
                    Status = status,
                    StatusName = OrderDisplayLabels.GetOrderStatusName(status, languageId),
                    Count = count
                };
            }
        }
    }
}
