using Application.Features.CustomerSection.Feature.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Wallet.Queries
{
    public sealed record GetCustomerWalletTransactionsQuery() : IRequest<Result<List<WalletTransactionDto>>>
    {
        private class GetCustomerWalletTransactionsQueryHandler : IRequestHandler<GetCustomerWalletTransactionsQuery,
                                                                                  Result<List<WalletTransactionDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetCustomerWalletTransactionsQueryHandler(INaqlahContext context,
                                                             IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<List<WalletTransactionDto>>> Handle(GetCustomerWalletTransactionsQuery request,
                                                                         CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;

                var customerId = await context.Customers
                                              .Where(x => x.UserId == userSession.UserId)
                                              .Select(x => x.Id)
                                              .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<List<WalletTransactionDto>>("Customer not found for the current user");
                }

                var transactions = await context.WalletTransctions
                                                .Where(x => x.CustomerId == customerId)
                                                .OrderByDescending(x => x.CreatedDate)
                                                .ToListAsync(cancellationToken);

                var result = transactions.Select(x => new WalletTransactionDto
                {
                    Id = x.Id,
                    Description = languageId == (int)Language.Arabic ? x.ArabicDescription : x.EnglishDescription,
                    Amount = x.Amount,
                    IsWithdraw = x.Withdraw,
                    Date = x.CreatedDate,
                    OrderId = x.OrderId,
                    TransactionType = ResolveTransactionType(x.Withdraw, x.OrderId)
                }).ToList();

                return Result.Success(result);
            }

            // No explicit type column on the wallet transaction, so the type is derived:
            //  - debit                  -> payment
            //  - credit tied to order   -> refund
            //  - credit with no order   -> charge (top-up)
            private static string ResolveTransactionType(bool isWithdraw, int? orderId)
            {
                if (isWithdraw)
                {
                    return "payment";
                }

                return orderId.HasValue ? "refund" : "charge";
            }
        }
    }
}
