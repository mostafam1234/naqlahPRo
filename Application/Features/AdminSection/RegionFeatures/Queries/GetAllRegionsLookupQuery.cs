using Application.Features.AdminSection.RegionFeatures.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.RegionFeatures.Queries
{
    public sealed record GetAllRegionsLookupQuery: IRequest<Result<List<RegionLookupDto>>>
    {
        public int LanguageId { get; init; }

        private class GetAllRegionsLookupQueryHandler : IRequestHandler<GetAllRegionsLookupQuery, Result<List<RegionLookupDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllRegionsLookupQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<List<RegionLookupDto>>> Handle(GetAllRegionsLookupQuery request, CancellationToken cancellationToken)
            {
                var regions = await _context.Regions
                    .Select(x => new RegionLookupDto
                    {
                        Id = x.Id,
                        Name = request.LanguageId == (int)Language.Arabic ? x.ArabicName : x.EnglishName
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(regions);
            }
        }
    }
}


