using Application.Features.CustomerSection.Feature.Region.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Region.Queries
{
    public sealed record GetOrderRegionQuery:IRequest<Result<List<RegionDto>>>
    {
        private class GetOrderRegionQueryHandler : IRequestHandler<GetOrderRegionQuery,
                                                                   Result<List<RegionDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetOrderRegionQueryHandler(INaqlahContext context,
                                              IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result<List<RegionDto>>> Handle(GetOrderRegionQuery request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                var regions=await context.Regions
                                                  .Select(x=>new RegionDto
                                                  {
                                                      Id=x.Id,
                                                      Name=languageId==(int)Domain.Enums.Language.Arabic?
                                                      x.ArabicName:x.EnglishName,
                                                      Cities=x.Cities.Select(c=>new CityDto
                                                      {
                                                          Id=c.Id,
                                                          Name=languageId==(int)Domain.Enums.Language.Arabic?
                                                          c.ArabicName:c.EnglishName,
                                                          RegionId=c.RegionId,
                                                          Neighborhoods =c.Neighborhoods.Select(n=>new NeighborhoodDto
                                                          {
                                                              Id=n.Id,
                                                              Name=languageId==(int)Domain.Enums.Language.Arabic?
                                                              n.ArabicName:n.EnglishName,
                                                                CityId=n.CityId
                                                          }).ToList()
                                                      }).ToList()
                                                  })
                                                  .ToListAsync(cancellationToken);

              
                return Result.Success(regions);
            }
        }
    }
}
