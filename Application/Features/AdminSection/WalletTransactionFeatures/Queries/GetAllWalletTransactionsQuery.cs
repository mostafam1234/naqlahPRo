using Application.Features.AdminSection.WalletTransactionFeatures.Dtos;
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

namespace Application.Features.AdminSection.WalletTransactionFeatures.Queries
{
    public sealed record GetAllWalletTransactionsQuery : IRequest<Result<PagedResult<WalletTransactionAdminDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public int? CustomerId { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool? Withdraw { get; init; }

        private class GetAllWalletTransactionsQueryHandler : IRequestHandler<GetAllWalletTransactionsQuery, Result<PagedResult<WalletTransactionAdminDto>>>
        {
            private readonly INaqlahContext _context;
            public GetAllWalletTransactionsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<PagedResult<WalletTransactionAdminDto>>> Handle(GetAllWalletTransactionsQuery request, CancellationToken cancellationToken)
            {
                if (!request.CustomerId.HasValue)
                {
                    return Result.Failure<PagedResult<WalletTransactionAdminDto>>("Customer ID is required");
                }

                var query = _context.WalletTransctions
                    .Where(x => x.CustomerId == request.CustomerId.Value)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicDescription.Contains(request.SearchTerm) ||
                                           x.EnglishDescription.Contains(request.SearchTerm));
                }

                if (request.FromDate.HasValue)
                {
                    // Normalize to start of day in UTC
                    var fromDate = request.FromDate.Value.Date.ToUniversalTime();
                    query = query.Where(x => x.CreatedDate >= fromDate);
                }

                if (request.ToDate.HasValue)
                {
                    // Normalize to end of day in UTC
                    var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(x => x.CreatedDate <= toDate);
                }

                if (request.Withdraw.HasValue)
                {
                    query = query.Where(x => x.Withdraw == request.Withdraw.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var transactions = await (from wt in query
                                         join c in _context.Customers on wt.CustomerId equals c.Id
                                         join u in _context.Users on c.UserId equals u.Id
                                         orderby wt.CreatedDate descending, wt.Id descending
                                         select new WalletTransactionAdminDto
                                         {
                                             Id = wt.Id,
                                             ArabicDescription = wt.ArabicDescription,
                                             EnglishDescription = wt.EnglishDescription,
                                             Amount = wt.Amount,
                                             Withdraw = wt.Withdraw,
                                             OrderId = wt.OrderId,
                                             CustomerId = wt.CustomerId,
                                             CustomerName = u.UserName ?? "غير محدد",
                                             CustomerPhoneNumber = c.PhoneNumber,
                                             CreatedDate = wt.CreatedDate
                                         })
                                         .Skip(request.Skip)
                                         .Take(request.Take)
                                         .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<WalletTransactionAdminDto>
                {
                    Data = transactions,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(pagedResult);
            }
        }
    }
}

