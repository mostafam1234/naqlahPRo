using Application.Features.AdminSection.AssistantWork.Dtos;
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

namespace Application.Features.AdminSection.AssistantWork.Queries
{
    public sealed record GetAllAssistantWorksQuery : IRequest<Result<PagedResult<AssistantWorkAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllAssistantWorksQueryHandler : IRequestHandler<GetAllAssistantWorksQuery, Result<PagedResult<AssistantWorkAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllAssistantWorksQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<PagedResult<AssistantWorkAdminDto>>> Handle(GetAllAssistantWorksQuery request, CancellationToken cancellationToken)
            {
                var query = _context.AssistanWorks.Where(x => !x.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var assistantWorks = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new AssistantWorkAdminDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                        Cost = x.Cost
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<AssistantWorkAdminDto>
                {
                    Data = assistantWorks,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

