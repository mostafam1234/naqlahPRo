using Application.Features.AdminSection.WalletTransactionFeatures.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.WalletTransactionFeatures.Queries
{
    /// <summary>
    /// Returns all wallet transactions for admin export (no CustomerId required). Optional From/To date filter.
    /// </summary>
    public sealed record GetAllWalletTransactionsForExportQuery : IRequest<Result<List<WalletTransactionAdminDto>>>
    {
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int MaxRows { get; init; } = 50000;

        private class Handler : IRequestHandler<GetAllWalletTransactionsForExportQuery, Result<List<WalletTransactionAdminDto>>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<WalletTransactionAdminDto>>> Handle(GetAllWalletTransactionsForExportQuery request, CancellationToken cancellationToken)
            {
                var query = from wt in _context.WalletTransctions
                            join c in _context.Customers on wt.CustomerId equals c.Id
                            join u in _context.Users on c.UserId equals u.Id
                            select new { wt, c, u };

                if (request.FromDate.HasValue)
                {
                    var fromDate = request.FromDate.Value.Date.ToUniversalTime();
                    query = query.Where(x => x.wt.CreatedDate >= fromDate);
                }

                if (request.ToDate.HasValue)
                {
                    var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(x => x.wt.CreatedDate <= toDate);
                }

                var list = await query
                    .OrderByDescending(x => x.wt.CreatedDate)
                    .Take(request.MaxRows)
                    .Select(x => new WalletTransactionAdminDto
                    {
                        Id = x.wt.Id,
                        ArabicDescription = x.wt.ArabicDescription,
                        EnglishDescription = x.wt.EnglishDescription,
                        Amount = x.wt.Amount,
                        Withdraw = x.wt.Withdraw,
                        OrderId = x.wt.OrderId,
                        CustomerId = x.wt.CustomerId,
                        CustomerName = x.u.UserName ?? "غير محدد",
                        CustomerPhoneNumber = x.c.PhoneNumber ?? string.Empty,
                        CreatedDate = x.wt.CreatedDate
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(list);
            }
        }
    }
}
