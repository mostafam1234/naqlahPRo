using Application.Features.AdminSection.RegionFeatures.Dtos;
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

namespace Application.Features.AdminSection.RegionFeatures.Queries
{
    public sealed record GetAllRegionsQuery: IRequest<Result<PagedResult<RegionAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllRegionsQueryHandler : IRequestHandler<GetAllRegionsQuery, Result<PagedResult<RegionAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllRegionsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<PagedResult<RegionAdminDto>>> Handle(GetAllRegionsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Regions.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var regions = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new RegionAdminDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<RegionAdminDto>
                {
                    Data = regions,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }

    }
}


