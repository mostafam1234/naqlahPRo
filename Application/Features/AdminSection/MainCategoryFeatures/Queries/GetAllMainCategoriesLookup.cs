using Application.Features.AdminSection.MainCategoryFeatures.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.MainCategoryFeatures.Queries
{
    public sealed record GetAllMainCategoriesLookup: IRequest<Result<List<MainCategoryAdminLookupDto>>>
    {
        public int LanguageId { get; set; } = 1;
        private class GetAllMainCategoriesLookupHandler: IRequestHandler<GetAllMainCategoriesLookup, Result<List<MainCategoryAdminLookupDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllMainCategoriesLookupHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<List<MainCategoryAdminLookupDto>>> Handle(GetAllMainCategoriesLookup request, CancellationToken cancellationToken)
            {
                var categories = await _context.MainCategories
                    .Select(x => new MainCategoryAdminLookupDto
                    {
                        Id = x.Id,
                        Name = request.LanguageId == 1 ? x.ArabicName : x.EnglishName
                    })
                    .ToListAsync();

                return Result.Success(categories);
            }
        }
    }
}
