using Application.Features.DeliveryManSection.Regestration.Dtos;
using Application.Features.VehicleSection.Dtos;
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

namespace Application.Features.VehicleSection.Queries
{
    public sealed record GetVehiclesBrandsForDisplaying : IRequest<Result<PagedResult<DeliveryManVehicleDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetVehiclesBrandsForDisplayingHandler : IRequestHandler<GetVehiclesBrandsForDisplaying, Result<PagedResult<DeliveryManVehicleDto>>>
        {
            private readonly INaqlahContext context;

            public GetVehiclesBrandsForDisplayingHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<PagedResult<DeliveryManVehicleDto>>> Handle(GetVehiclesBrandsForDisplaying request, CancellationToken cancellationToken)
            {
                var query = context.VehicleBrands.AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var brands = await query
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
                    Data = brands,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
