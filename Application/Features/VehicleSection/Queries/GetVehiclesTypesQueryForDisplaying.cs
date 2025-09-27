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

            public GetVehiclesTypesQueryForDisplayingHandler(INaqlahContext context,
                                                            IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<PagedResult<DeliveryManVehicleDto>>> Handle(GetVehiclesTypesQueryForDisplaying request, CancellationToken cancellationToken)
            {
                var query = context.VehicleTypes.AsQueryable();

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
