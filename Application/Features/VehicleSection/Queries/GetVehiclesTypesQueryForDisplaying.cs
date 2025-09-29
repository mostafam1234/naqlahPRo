using Application.Features.VehicleSection.Dtos;
using Application.Features.VehicleSection.Queries;
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

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var types = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new DeliveryManVehicleDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                        IconImagePath = $"{baseUrl}/ImageBank/{VehicleFolderPrefix}/{x.IconImagePath}",
                        MainCategories = x.VehicleTypeCategoies.Select(vtc => new MainCategoryInfo
                        {
                            Id = vtc.MainCategory.Id,
                            ArabicName = vtc.MainCategory.ArabicName,
                            EnglishName = vtc.MainCategory.EnglishName
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<DeliveryManVehicleDto>
                {
                    Data = types,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}
