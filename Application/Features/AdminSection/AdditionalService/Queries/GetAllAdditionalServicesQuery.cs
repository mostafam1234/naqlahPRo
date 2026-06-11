using Application.Features.AdminSection.AdditionalService.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.AdditionalService.Queries
{
    public sealed record GetAllAdditionalServicesQuery : IRequest<Result<PagedResult<AdditionalServiceAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllAdditionalServicesQueryHandler :
            IRequestHandler<GetAllAdditionalServicesQuery, Result<PagedResult<AdditionalServiceAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllAdditionalServicesQueryHandler(INaqlahContext context) => _context = context;

            public async Task<Result<PagedResult<AdditionalServiceAdminDto>>> Handle(
                GetAllAdditionalServicesQuery request, CancellationToken cancellationToken)
            {
                var query = _context.AdditionalServices.Where(x => !x.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                            x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderBy(x => x.Id)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new AdditionalServiceAdminDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                        UnitPrice = x.UnitPrice
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(new PagedResult<AdditionalServiceAdminDto>
                {
                    Data = items,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.Take)
                });
            }
        }
    }
}
