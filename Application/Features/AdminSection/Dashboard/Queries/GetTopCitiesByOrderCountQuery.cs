using Application.Features.AdminSection.Dashboard.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Queries
{
    public sealed record GetTopCitiesByOrderCountQuery(int LanguageId) : IRequest<Result<List<CityOrderCountDto>>>
    {
        private class GetTopCitiesByOrderCountQueryHandler : IRequestHandler<GetTopCitiesByOrderCountQuery, Result<List<CityOrderCountDto>>>
        {
            private readonly INaqlahContext _context;

            public GetTopCitiesByOrderCountQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<CityOrderCountDto>>> Handle(GetTopCitiesByOrderCountQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                // Load orders with waypoints and cities, then process in memory
                // This approach works around EF Core translation limitations
                var ordersWithWaypoints = await _context.Orders
                    .Include(o => o.OrderWayPoints)
                        .ThenInclude(owp => owp.City)
                    .ToListAsync(cancellationToken);

                // Process in memory: get all origin waypoints, group by city
                var cityOrderCounts = ordersWithWaypoints
                    .SelectMany(o => o.OrderWayPoints)
                    .Where(owp => owp.IsOrgin && owp.City != null)
                    .GroupBy(owp => new { owp.CityId, City = owp.City })
                    .Select(g => new
                    {
                        CityId = g.Key.CityId,
                        CityName = isArabic ? g.Key.City.ArabicName : g.Key.City.EnglishName,
                        OrderCount = g.Select(owp => owp.OrderId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(4)
                    .ToList();

                // Calculate total orders for percentage calculation
                var totalOrders = cityOrderCounts.Sum(x => x.OrderCount);
                
                // Calculate percentage for each city
                var result = cityOrderCounts.Select(x => new CityOrderCountDto
                {
                    CityId = x.CityId,
                    CityName = x.CityName,
                    OrderCount = x.OrderCount,
                    Percentage = totalOrders > 0 ? Math.Round((double)x.OrderCount / totalOrders * 100, 2) : 0
                }).ToList();

                return Result.Success(result);
            }
        }
    }
}

