using Application.Features.AdminSection.MainCategoryFeatures.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.MainCategoryFeatures.Queries
{
    public sealed record GetVehicleTypesByMainCategoryIdQuery : IRequest<Result<MainCategoryVehicleTypesResultDto>>
    {
        public int MainCategoryId { get; init; }

        private sealed class Handler : IRequestHandler<GetVehicleTypesByMainCategoryIdQuery, Result<MainCategoryVehicleTypesResultDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<MainCategoryVehicleTypesResultDto>> Handle(
                GetVehicleTypesByMainCategoryIdQuery request,
                CancellationToken cancellationToken)
            {
                var mainCategory = await _context.MainCategories
                    .AsNoTracking()
                    .Where(mc => mc.Id == request.MainCategoryId)
                    .Select(mc => new
                    {
                        mc.Id,
                        mc.ArabicName,
                        mc.EnglishName
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (mainCategory is null)
                {
                    return Result.Failure<MainCategoryVehicleTypesResultDto>("Main category not found");
                }

                var vehicleTypes = await _context.VehicleTypeCategories
                    .AsNoTracking()
                    .Where(vtc => vtc.MainCategoryId == request.MainCategoryId)
                    .Select(vtc => vtc.VehicleType)
                    .OrderBy(vt => vt.ArabicName)
                    .Select(vt => new MainCategoryVehicleTypeDto
                    {
                        Id = vt.Id,
                        ArabicName = vt.ArabicName,
                        EnglishName = vt.EnglishName,
                        LoadCategory = vt.LoadCategory,
                        Cost = vt.Cost
                    })
                    .ToListAsync(cancellationToken);

                foreach (var vehicleType in vehicleTypes)
                {
                    if (!vehicleType.LoadCategory.HasValue)
                    {
                        continue;
                    }

                    vehicleType.LoadCategoryArabicName = VehicleDisplayLabels.GetLoadCategoryName(
                        vehicleType.LoadCategory.Value,
                        (int)Language.Arabic);
                    vehicleType.LoadCategoryEnglishName = VehicleDisplayLabels.GetLoadCategoryName(
                        vehicleType.LoadCategory.Value,
                        (int)Language.English);
                }

                return Result.Success(new MainCategoryVehicleTypesResultDto
                {
                    MainCategoryId = mainCategory.Id,
                    MainCategoryArabicName = mainCategory.ArabicName,
                    MainCategoryEnglishName = mainCategory.EnglishName,
                    VehicleTypes = vehicleTypes
                });
            }
        }
    }
}
