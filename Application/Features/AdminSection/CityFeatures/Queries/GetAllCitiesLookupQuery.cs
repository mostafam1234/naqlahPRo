using Application.Features.AdminSection.CityFeatures.Dtos;
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

namespace Application.Features.AdminSection.CityFeatures.Queries
{
    public sealed record GetAllCitiesLookupQuery: IRequest<Result<List<CityLookupDto>>>
    {
        public int LanguageId { get; init; }
        public int? RegionId { get; init; }

        private class GetAllCitiesLookupQueryHandler : IRequestHandler<GetAllCitiesLookupQuery, Result<List<CityLookupDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllCitiesLookupQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<List<CityLookupDto>>> Handle(GetAllCitiesLookupQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Cities.AsQueryable();

                if (request.RegionId.HasValue)
                {
                    query = query.Where(x => x.RegionId == request.RegionId.Value);
                }

                var cities = await query
                    .Select(x => new CityLookupDto
                    {
                        Id = x.Id,
                        Name = request.LanguageId == (int)Language.Arabic ? x.ArabicName : x.EnglishName
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(cities);
            }
        }
    }
}


