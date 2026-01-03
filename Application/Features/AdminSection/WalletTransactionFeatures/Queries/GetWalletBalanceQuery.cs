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
    public sealed record GetWalletBalanceQuery : IRequest<Result<decimal>>
    {
        public int CustomerId { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool? Withdraw { get; init; }

        private class GetWalletBalanceQueryHandler : IRequestHandler<GetWalletBalanceQuery, Result<decimal>>
        {
            private readonly INaqlahContext _context;
            public GetWalletBalanceQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<decimal>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
            {
                var query = _context.WalletTransctions
                    .Where(x => x.CustomerId == request.CustomerId)
                    .AsQueryable();

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

                var balance = await query
                    .SumAsync(x => x.Withdraw ? -x.Amount : x.Amount, cancellationToken);

                return Result.Success(balance);
            }
        }
    }
}



