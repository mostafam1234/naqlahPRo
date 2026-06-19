using Application.Features.DeliveryManSection.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Wallet.Queries
{
    public sealed record GetDeliveryManWalletBalanceQuery() : IRequest<Result<DeliveryManWalletBalanceDto>>
    {
        private class Handler : IRequestHandler<GetDeliveryManWalletBalanceQuery, Result<DeliveryManWalletBalanceDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public Handler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<DeliveryManWalletBalanceDto>> Handle(GetDeliveryManWalletBalanceQuery request,
                                                                          CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure<DeliveryManWalletBalanceDto>("Delivery man not found");
                }

                var transactions = await context.DeliveryManWalletTransactions
                    .Where(x => x.DeliveryManId == deliveryManId)
                    .ToListAsync(cancellationToken);

                var balance = transactions
                    .Where(x => x.Status == DeliveryManWalletTransaction.StatusCompleted)
                    .Sum(x => x.IsCredit ? x.Amount : -x.Amount);

                var pendingBalance = transactions
                    .Where(x => x.Status == DeliveryManWalletTransaction.StatusPending && !x.IsCredit)
                    .Sum(x => x.Amount);

                return Result.Success(new DeliveryManWalletBalanceDto
                {
                    Balance = balance,
                    AvailableBalance = balance,
                    PendingBalance = pendingBalance
                });
            }
        }
    }
}
