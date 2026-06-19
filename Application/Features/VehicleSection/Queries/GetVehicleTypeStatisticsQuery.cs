using Application.Features.VehicleSection.Dtos;
using Application.Shared.Services;
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

namespace Application.Features.VehicleSection.Queries
{
    public sealed record GetVehicleTypeStatisticsQuery : IRequest<Result<VehicleTypeStatisticsDto>>
    {
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetVehicleTypeStatisticsQuery, Result<VehicleTypeStatisticsDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<VehicleTypeStatisticsDto>> Handle(
                GetVehicleTypeStatisticsQuery request,
                CancellationToken cancellationToken)
            {
                var totalVehicleTypes = await _context.VehicleTypes.CountAsync(cancellationToken);

                var typeCounts = await _context.VehicleTypes
                    .AsNoTracking()
                    .GroupBy(vt => vt.LoadCategory)
                    .Select(g => new { LoadCategory = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var typeCountLookup = typeCounts.ToDictionary(x => x.LoadCategory, x => x.Count);

                var registeredQuery = from dv in _context.DeliveryVehicles.AsNoTracking()
                                      join dm in _context.DeliveryMen.AsNoTracking() on dv.DeliveryManId equals dm.Id
                                      join vt in _context.VehicleTypes.AsNoTracking() on dv.VehicleTypeId equals vt.Id
                                      select new { vt.LoadCategory, dm.RegisteredAt };

                if (request.FromDate.HasValue)
                {
                    var fromDate = request.FromDate.Value.Date.ToUniversalTime();
                    registeredQuery = registeredQuery.Where(x => x.RegisteredAt >= fromDate);
                }

                if (request.ToDate.HasValue)
                {
                    var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    registeredQuery = registeredQuery.Where(x => x.RegisteredAt <= toDate);
                }

                var registeredCounts = await registeredQuery
                    .GroupBy(x => x.LoadCategory)
                    .Select(g => new { LoadCategory = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var registeredLookup = registeredCounts.ToDictionary(x => x.LoadCategory, x => x.Count);
                var totalRegistered = registeredCounts.Sum(x => x.Count);

                var loadCategoryCounts = new List<VehicleLoadCategoryCountDto>();

                foreach (var category in VehicleDisplayLabels.AllLoadCategories)
                {
                    typeCountLookup.TryGetValue(category, out var typeCount);
                    registeredLookup.TryGetValue(category, out var registeredCount);

                    loadCategoryCounts.Add(new VehicleLoadCategoryCountDto
                    {
                        LoadCategory = (int)category,
                        LoadCategoryName = VehicleDisplayLabels.GetLoadCategoryName(category, request.LanguageId),
                        VehicleTypeCount = typeCount,
                        RegisteredVehicleCount = registeredCount
                    });
                }

                typeCountLookup.TryGetValue(null, out var uncategorizedTypes);
                registeredLookup.TryGetValue(null, out var uncategorizedRegistered);

                if (uncategorizedTypes > 0 || uncategorizedRegistered > 0)
                {
                    loadCategoryCounts.Add(new VehicleLoadCategoryCountDto
                    {
                        LoadCategory = null,
                        LoadCategoryName = request.LanguageId == (int)Language.Arabic ? "بدون تصنيف" : "Uncategorized",
                        VehicleTypeCount = uncategorizedTypes,
                        RegisteredVehicleCount = uncategorizedRegistered
                    });
                }

                return Result.Success(new VehicleTypeStatisticsDto
                {
                    TotalVehicleTypes = totalVehicleTypes,
                    TotalRegisteredVehicles = totalRegistered,
                    LoadCategoryCounts = loadCategoryCounts
                });
            }
        }
    }
}
