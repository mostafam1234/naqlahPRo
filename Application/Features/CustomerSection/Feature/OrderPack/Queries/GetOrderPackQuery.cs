using Application.Features.CustomerSection.Feature.OrderPack.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.OrderPack.Queries
{
    public sealed record GetOrderPackQuery:IRequest<Result<List<OrderPackDto>>>
    {
        private class GetOrderPackQueryHandler : IRequestHandler<GetOrderPackQuery,
                                                                 Result<List<OrderPackDto>>>
        {
            private readonly INaqlahContext naqlahContext;
            private readonly IUserSession userSession;

            public GetOrderPackQueryHandler(INaqlahContext naqlahContext,
                                            IUserSession userSession)
            {
                this.naqlahContext = naqlahContext;
                this.userSession = userSession;
            }
            public async Task<Result<List<OrderPackDto>>> Handle(GetOrderPackQuery request, CancellationToken cancellationToken)
            {
                var orderPacks=await naqlahContext.OrderPackages
                                                  .Select(x=>new OrderPackDto
                                                  {
                                                      Id=x.Id,
                                                      Description=userSession.LanguageId==(int)Domain.Enums.Language.Arabic?
                                                      x.ArabicDescripton:x.EnglishDescription
                                                  })
                                                  .ToListAsync(cancellationToken);
              
                return Result.Success(orderPacks);
            }
        }
    }
}
