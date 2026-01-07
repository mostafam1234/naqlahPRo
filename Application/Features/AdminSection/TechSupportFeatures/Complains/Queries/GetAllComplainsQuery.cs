using Application.Features.AdminSection.TechSupportFeatures.Complains.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.TechSupportFeatures.Complains.Queries
{
    public sealed record GetAllComplainsQuery : IRequest<Result<PagedResult<ComplainDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllComplainsQueryHandler : IRequestHandler<GetAllComplainsQuery, Result<PagedResult<ComplainDto>>>
        {
            private readonly INaqlahContext _context;
            
            public GetAllComplainsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<ComplainDto>>> Handle(GetAllComplainsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Complains.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.CustomerName.Contains(request.SearchTerm) ||
                                           x.CustomerMobileNumber.Contains(request.SearchTerm) ||
                                           x.Description.Contains(request.SearchTerm) ||
                                           x.CustomerAddress.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var complains = await query
                    .OrderByDescending(x => x.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new ComplainDto
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

                var pagedResult = new PagedResult<ComplainDto>
                {
                    Data = complains,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

