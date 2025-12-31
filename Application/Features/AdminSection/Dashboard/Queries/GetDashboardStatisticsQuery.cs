using Application.Features.AdminSection.Dashboard.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Queries
{
    public sealed record GetDashboardStatisticsQuery : IRequest<Result<DashboardStatisticsDto>>
    {
        private class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, Result<DashboardStatisticsDto>>
        {
            private readonly INaqlahContext _context;

            public GetDashboardStatisticsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<DashboardStatisticsDto>> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                var now = DateTime.UtcNow;
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                // Calculate start of week (Sunday = 0, so we subtract the day of week)
                var dayOfWeek = (int)now.DayOfWeek;
                var weekStart = todayStart.AddDays(-dayOfWeek);

                    // Total orders
                    var totalOrders = await _context.Orders.CountAsync(cancellationToken);

                    // Today's orders
                    var todayOrders = await _context.Orders
                        .Where(o => o.CreationDate >= todayStart)
                        .CountAsync(cancellationToken);

                    // This week's orders
                    var thisWeekOrders = await _context.Orders
                        .Where(o => o.CreationDate >= weekStart)
                        .CountAsync(cancellationToken);

                    // Total customers
                    var totalCustomers = await _context.Customers.CountAsync(cancellationToken);

                    var statistics = new DashboardStatisticsDto
                    {
                        TotalOrders = totalOrders,
                        TodayOrders = todayOrders,
                        ThisWeekOrders = thisWeekOrders,
                        TotalCustomers = totalCustomers
                    };

                    return Result.Success(statistics);
                }
                catch (Exception ex)
                {
                    return Result.Failure<DashboardStatisticsDto>($"Error retrieving dashboard statistics: {ex.Message}");
                }
            }
        }
    }
}

