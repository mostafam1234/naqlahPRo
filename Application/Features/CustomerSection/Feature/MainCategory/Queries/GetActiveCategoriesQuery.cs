using Application.Features.CustomerSection.Feature.MainCategory.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.MainCategory.Queries
{
    public sealed record GetActiveCategoriesQuery:IRequest<Result<List<ActiveCategoryDto>>>
    {
        private class GetActiveCategoriesQueryHandler : IRequestHandler<GetActiveCategoriesQuery,
                                                                       Result<List<ActiveCategoryDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetActiveCategoriesQueryHandler(INaqlahContext context,
                                                   IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result<List<ActiveCategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                var categories = await context.MainCategories
                                              .Select(x => new ActiveCategoryDto
                                              {
                                                  Id = x.Id,
                                                  Name = languageId==(int)Language.Arabic?
                                                  x.ArabicName:x.EnglishName
                                              })
                                              .ToListAsync(cancellationToken);

                return Result.Success(categories);
            }
        }
    }
}
