using Application.Features.AdminSection.DiscountCodeFeatures.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.DiscountCodeFeatures.Queries
{
    public sealed record GetAllDiscountCodesQuery : IRequest<Result<PagedResult<DiscountCodeAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public bool? IsActive { get; init; }

        private class Handler : IRequestHandler<GetAllDiscountCodesQuery, Result<PagedResult<DiscountCodeAdminDto>>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<DiscountCodeAdminDto>>> Handle(GetAllDiscountCodesQuery request, CancellationToken cancellationToken)
            {
                var query = _context.DiscountCodes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(x => x.Code.Contains(request.SearchTerm));

                if (request.IsActive.HasValue)
                    query = query.Where(x => x.IsActive == request.IsActive.Value);

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new DiscountCodeAdminDto
                    {
                        Id = x.Id,
                        Code = x.Code,
                        DiscountAmount = x.DiscountAmount,
                        IsActive = x.IsActive,
                        UsageLimit = x.UsageLimit,
                        UsageCount = x.UsageCount,
                        ExpiryDate = x.ExpiryDate,
                        MinimumOrderAmount = x.MinimumOrderAmount,
                        CreatedDate = x.CreatedDate
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(new PagedResult<DiscountCodeAdminDto>
                {
                    Data = items,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.Take)
                });
            }
        }
    }
}
