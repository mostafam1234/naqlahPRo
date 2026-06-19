using Application.Features.VehicleSection.Dtos;
using Application.Shared.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Application.Features.VehicleSection.Queries
{
    public sealed record GetVehiclesTypesQueryForDisplaying : IRequest<Result<PagedResult<DeliveryManVehicleDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetVehiclesTypesQueryForDisplayingHandler : IRequestHandler<GetVehiclesTypesQueryForDisplaying, Result<PagedResult<DeliveryManVehicleDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            readonly IReadFromAppSetting config;
            private const string VehicleFolderPrefix = "vehicle-types";

            public GetVehiclesTypesQueryForDisplayingHandler(INaqlahContext context,
                                                            IUserSession userSession,
                                                            IReadFromAppSetting config)
            {
                this.config = config;
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<PagedResult<DeliveryManVehicleDto>>> Handle(GetVehiclesTypesQueryForDisplaying request, CancellationToken cancellationToken)
            {
                var baseUrl = config.GetValue<string>("apiBaseUrl");
                var query = context.VehicleTypes
                    .Include(vt => vt.VehicleTypeCategoies)
                        .ThenInclude(vtc => vtc.MainCategory)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var types = await query
                    .OrderByDescending(x => x.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new DeliveryManVehicleDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                        IconImagePath = string.IsNullOrEmpty(x.IconImagePath)
                            ? string.Empty
                            : $"{baseUrl}/ImageBank/{VehicleFolderPrefix}/{x.IconImagePath}",
                        Cost = x.Cost,
                        ServiceFee = x.ServiceFee,
                        LoadCategory = x.LoadCategory,
                        CreationDate = x.CreationDate,
                        MainCategories = x.VehicleTypeCategoies.Select(vtc => new MainCategoryInfo
                        {
                            Id = vtc.MainCategory.Id,
                            ArabicName = vtc.MainCategory.ArabicName,
                            EnglishName = vtc.MainCategory.EnglishName,
                            Name = userSession.LanguageId == 1 ? vtc.MainCategory.ArabicName : vtc.MainCategory.EnglishName
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                foreach (var type in types)
                {
                    if (type.LoadCategory.HasValue)
                    {
                        type.LoadCategoryName = VehicleDisplayLabels.GetLoadCategoryName(
                            type.LoadCategory.Value,
                            userSession.LanguageId);
                    }
                }

                var totalPages = request.Take > 0 ? (int)Math.Ceiling((double)totalCount / request.Take) : 0;

                return Result.Success(new PagedResult<DeliveryManVehicleDto>
                {
                    Data = types,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                });
            }
        }
    }
}
