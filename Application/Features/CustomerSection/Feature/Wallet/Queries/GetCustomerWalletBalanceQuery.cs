using Application.Features.CustomerSection.Feature.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Wallet.Queries
{
    public sealed record GetCustomerWalletBalanceQuery() : IRequest<Result<CustomerWalletBalanceDto>>
    {
        private class GetCustomerWalletBalanceQueryHandler : IRequestHandler<GetCustomerWalletBalanceQuery,
                                                                            Result<CustomerWalletBalanceDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetCustomerWalletBalanceQueryHandler(INaqlahContext context,
                                                        IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<CustomerWalletBalanceDto>> Handle(GetCustomerWalletBalanceQuery request, CancellationToken cancellationToken)
            {
                var customerId = await context.Customers
                                              .Where(x => x.UserId == userSession.UserId)
                                              .Select(x => x.Id)
                                              .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<CustomerWalletBalanceDto>("Customer not found for the current user");
                }

                var customer = await context.Customers
                                           .Include(c => c.WalletTransctions)
                                           .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

                if (customer == null)
                {
                    return Result.Failure<CustomerWalletBalanceDto>("Customer not found");
                }

                var wallletTransctions = customer.WalletTransctions;

                var customerBalance = wallletTransctions.Any() ?
                    wallletTransctions.Sum(x => x.Withdraw ? -x.Amount : x.Amount) : 0;


                var result = new CustomerWalletBalanceDto
                {
                    CustomerId = customerId,
                    Balance = customerBalance,
                    TransactionCount = customer.WalletTransctions.Count
                };

                return Result.Success(result);
            }
        }
    }
}