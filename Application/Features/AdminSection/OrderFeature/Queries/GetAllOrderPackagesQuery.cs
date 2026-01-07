using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public sealed record GetAllOrderPackagesQuery : IRequest<Result<PagedResult<OrderPackageDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllOrderPackagesQueryHandler : IRequestHandler<GetAllOrderPackagesQuery, Result<PagedResult<OrderPackageDto>>>
        {
            private readonly INaqlahContext _context;

            public GetAllOrderPackagesQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<OrderPackageDto>>> Handle(GetAllOrderPackagesQuery request, CancellationToken cancellationToken)
            {
                var query = _context.OrderPackages.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicDescripton.Contains(request.SearchTerm) ||
                                           x.EnglishDescription.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var orderPackages = await query
                    .OrderBy(x => x.Id)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new OrderPackageDto
                    {
                        Id = x.Id,
                        ArabicDescription = x.ArabicDescripton,
                        EnglishDescription = x.EnglishDescription,
                        MinWeightInKg = x.MinWeightInKiloGram,
                        MaxWeightInKg = x.MaxWeightInKiloGram
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<OrderPackageDto>
                {
                    Data = orderPackages,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

