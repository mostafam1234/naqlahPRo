using Application.Features.VehicleSection.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.VehicleSection.Queries
{
    public sealed record GetVehicleLoadCategoriesLookupQuery : IRequest<Result<List<VehicleLoadCategoryLookupDto>>>
    {
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetVehicleLoadCategoriesLookupQuery, Result<List<VehicleLoadCategoryLookupDto>>>
        {
            public Task<Result<List<VehicleLoadCategoryLookupDto>>> Handle(
                GetVehicleLoadCategoriesLookupQuery request,
                CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                var items = VehicleDisplayLabels.AllLoadCategories
                    .Select(category => new VehicleLoadCategoryLookupDto
                    {
                        Id = category,
                        ArabicName = VehicleDisplayLabels.GetLoadCategoryName(category, 1),
                        EnglishName = VehicleDisplayLabels.GetLoadCategoryName(category, 2),
                        Name = VehicleDisplayLabels.GetLoadCategoryName(category, request.LanguageId)
                    })
                    .ToList();

                return Task.FromResult(Result.Success(items));
            }
        }
    }
}
