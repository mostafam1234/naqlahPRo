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

                var registeredVehiclesQuery = _context.DeliveryVehicles.AsNoTracking();

                if (request.FromDate.HasValue || request.ToDate.HasValue)
                {
                    var fromDate = request.FromDate?.Date.ToUniversalTime();
                    var toDate = request.ToDate?.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    registeredVehiclesQuery = registeredVehiclesQuery.Where(dv =>
                        _context.DeliveryMen.Any(dm =>
                            dm.Id == dv.DeliveryManId &&
                            (!fromDate.HasValue || dm.RegisteredAt >= fromDate.Value) &&
                            (!toDate.HasValue || dm.RegisteredAt <= toDate.Value)));
                }

                var registeredVehicleRows = await registeredVehiclesQuery
                    .Select(dv => new { dv.Id, dv.VehicleTypeId })
                    .ToListAsync(cancellationToken);

                var totalRegistered = registeredVehicleRows.Count;

                var registeredCountsByType = registeredVehicleRows
                    .GroupBy(x => x.VehicleTypeId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var registeredByLoadCategory = new Dictionary<VehicleLoadCategory?, int>();

                if (registeredCountsByType.Count > 0)
                {
                    var typeLoadCategories = await _context.VehicleTypes
                        .AsNoTracking()
                        .Where(vt => registeredCountsByType.Keys.Contains(vt.Id))
                        .Select(vt => new { vt.Id, vt.LoadCategory })
                        .ToListAsync(cancellationToken);

                    foreach (var row in typeLoadCategories)
                    {
                        if (!registeredCountsByType.TryGetValue(row.Id, out var count))
                            continue;

                        registeredByLoadCategory.TryGetValue(row.LoadCategory, out var existing);
                        registeredByLoadCategory[row.LoadCategory] = existing + count;
                    }
                }

                var loadCategoryCounts = new List<VehicleLoadCategoryCountDto>();

                foreach (var category in VehicleDisplayLabels.AllLoadCategories)
                {
                    typeCountLookup.TryGetValue(category, out var typeCount);
                    registeredByLoadCategory.TryGetValue(category, out var registeredCount);

                    loadCategoryCounts.Add(new VehicleLoadCategoryCountDto
                    {
                        LoadCategory = (int)category,
                        LoadCategoryName = VehicleDisplayLabels.GetLoadCategoryName(category, request.LanguageId),
                        VehicleTypeCount = typeCount,
                        RegisteredVehicleCount = registeredCount
                    });
                }

                typeCountLookup.TryGetValue(null, out var uncategorizedTypes);
                registeredByLoadCategory.TryGetValue(null, out var uncategorizedRegistered);

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

                var mainCategories = await _context.MainCategories
                    .AsNoTracking()
                    .OrderBy(mc => mc.ArabicName)
                    .Select(mc => new
                    {
                        mc.Id,
                        mc.ArabicName,
                        mc.EnglishName
                    })
                    .ToListAsync(cancellationToken);

                var typeCategoryLinks = await _context.VehicleTypeCategories
                    .AsNoTracking()
                    .Select(vtc => new
                    {
                        vtc.VehicleTypeId,
                        vtc.MainCategoryId
                    })
                    .ToListAsync(cancellationToken);

                var vehicleTypeCountByCategory = typeCategoryLinks
                    .GroupBy(x => x.MainCategoryId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.VehicleTypeId).Distinct().Count());

                var registeredCountByCategory = new Dictionary<int, int>();

                if (registeredVehicleRows.Count > 0 && typeCategoryLinks.Count > 0)
                {
                    var categoriesByTypeId = typeCategoryLinks
                        .GroupBy(x => x.VehicleTypeId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.MainCategoryId).Distinct().ToList());

                    foreach (var vehicle in registeredVehicleRows)
                    {
                        if (!categoriesByTypeId.TryGetValue(vehicle.VehicleTypeId, out var categoryIds))
                            continue;

                        foreach (var categoryId in categoryIds)
                        {
                            registeredCountByCategory.TryGetValue(categoryId, out var current);
                            registeredCountByCategory[categoryId] = current + 1;
                        }
                    }
                }

                var isArabic = request.LanguageId == (int)Language.Arabic;
                var mainCategoryCounts = mainCategories
                    .Select(mc => new MainCategoryVehicleCountDto
                    {
                        MainCategoryId = mc.Id,
                        ArabicName = mc.ArabicName,
                        EnglishName = mc.EnglishName,
                        Name = isArabic ? mc.ArabicName : mc.EnglishName,
                        VehicleTypeCount = vehicleTypeCountByCategory.GetValueOrDefault(mc.Id),
                        RegisteredVehicleCount = registeredCountByCategory.GetValueOrDefault(mc.Id)
                    })
                    .ToList();

                return Result.Success(new VehicleTypeStatisticsDto
                {
                    TotalVehicleTypes = totalVehicleTypes,
                    TotalRegisteredVehicles = totalRegistered,
                    LoadCategoryCounts = loadCategoryCounts,
                    MainCategoryCounts = mainCategoryCounts
                });
            }
        }
    }
}
