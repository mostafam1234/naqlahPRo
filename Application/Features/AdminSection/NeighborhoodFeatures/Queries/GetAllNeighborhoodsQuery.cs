using Application.Features.AdminSection.NeighborhoodFeatures.Dtos;
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

namespace Application.Features.AdminSection.NeighborhoodFeatures.Queries
{
    public sealed record GetAllNeighborhoodsQuery: IRequest<Result<PagedResult<NeighborhoodAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public int? CityId { get; init; }

        private class GetAllNeighborhoodsQueryHandler : IRequestHandler<GetAllNeighborhoodsQuery, Result<PagedResult<NeighborhoodAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllNeighborhoodsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<PagedResult<NeighborhoodAdminDto>>> Handle(GetAllNeighborhoodsQuery request, CancellationToken cancellationToken)
            {
                var query = from neighborhood in _context.Neighborhoods
                            join city in _context.Cities on neighborhood.CityId equals city.Id
                            select new { neighborhood, city };

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.neighborhood.ArabicName.Contains(request.SearchTerm) ||
                                           x.neighborhood.EnglishName.Contains(request.SearchTerm));
                }

                if (request.CityId.HasValue)
                {
                    query = query.Where(x => x.neighborhood.CityId == request.CityId.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var neighborhoods = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new NeighborhoodAdminDto
                    {
                        Id = x.neighborhood.Id,
                        ArabicName = x.neighborhood.ArabicName,
                        EnglishName = x.neighborhood.EnglishName,
                        CityId = x.neighborhood.CityId,
                        CityName = x.city.ArabicName
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<NeighborhoodAdminDto>
                {
                    Data = neighborhoods,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }

    }
}


