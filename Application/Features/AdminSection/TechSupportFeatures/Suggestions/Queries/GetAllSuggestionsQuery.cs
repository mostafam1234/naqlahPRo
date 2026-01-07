using Application.Features.AdminSection.TechSupportFeatures.Suggestions.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.TechSupportFeatures.Suggestions.Queries
{
    public sealed record GetAllSuggestionsQuery : IRequest<Result<PagedResult<SuggestionDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllSuggestionsQueryHandler : IRequestHandler<GetAllSuggestionsQuery, Result<PagedResult<SuggestionDto>>>
        {
            private readonly INaqlahContext _context;
            
            public GetAllSuggestionsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<SuggestionDto>>> Handle(GetAllSuggestionsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Suggestions.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.CustomerName.Contains(request.SearchTerm) ||
                                           x.CustomerMobileNumber.Contains(request.SearchTerm) ||
                                           x.Description.Contains(request.SearchTerm) ||
                                           x.CustomerAddress.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var suggestions = await query
                    .OrderByDescending(x => x.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new SuggestionDto
                    {
                        Id = x.Id,
                        CustomerId = x.CustomerId,
                        CustomerName = x.CustomerName,
                        CustomerMobileNumber = x.CustomerMobileNumber,
                        CustomerAddress = x.CustomerAddress,
                        Description = x.Description,
                        CreationDate = x.CreationDate
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<SuggestionDto>
                {
                    Data = suggestions,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

