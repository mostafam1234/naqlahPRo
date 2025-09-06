using Application.Features.CustomerSection.Feature.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Wallet.Queries
{
    public sealed record GetCustomerWalletBalanceQuery(int CustomerId) : IRequest<Result<CustomerWalletBalanceDto>>
    {
        private class GetCustomerWalletBalanceQueryHandler : IRequestHandler<GetCustomerWalletBalanceQuery,
                                                                            Result<CustomerWalletBalanceDto>>
        {
            private readonly INaqlahContext context;

            public GetCustomerWalletBalanceQueryHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<CustomerWalletBalanceDto>> Handle(GetCustomerWalletBalanceQuery request, CancellationToken cancellationToken)
            {
                var customer = await context.Customers
                                           .Include(c => c.WalletTransctions)
                                           .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

                if (customer == null)
                {
                    return Result.Failure<CustomerWalletBalanceDto>("Customer not found");
                }

                var result = new CustomerWalletBalanceDto
                {
                    CustomerId = request.CustomerId,
                    Balance = customer.WalletBalance,
                    TransactionCount = customer.WalletTransctions.Count
                };

                return Result.Success(result);
            }
        }
    }
}