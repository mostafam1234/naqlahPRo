using Application.Features.AdminSection.CityFeatures.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CityFeatures.Queries
{
    public sealed record GetAllCitiesQuery: IRequest<Result<PagedResult<CityAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public int? RegionId { get; init; }

        private class GetAllCitiesQueryHandler : IRequestHandler<GetAllCitiesQuery, Result<PagedResult<CityAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllCitiesQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<PagedResult<CityAdminDto>>> Handle(GetAllCitiesQuery request, CancellationToken cancellationToken)
            {
                var query = from city in _context.Cities
                            join region in _context.Regions on city.RegionId equals region.Id
                            select new { city, region };

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.city.ArabicName.Contains(request.SearchTerm) ||
                                           x.city.EnglishName.Contains(request.SearchTerm));
                }

                if (request.RegionId.HasValue)
                {
                    query = query.Where(x => x.city.RegionId == request.RegionId.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var cities = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new CityAdminDto
                    {
                        Id = x.city.Id,
                        ArabicName = x.city.ArabicName,
                        EnglishName = x.city.EnglishName,
                        RegionId = x.city.RegionId,
                        RegionName = x.region.ArabicName
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<CityAdminDto>
                {
                    Data = cities,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }

    }
}

