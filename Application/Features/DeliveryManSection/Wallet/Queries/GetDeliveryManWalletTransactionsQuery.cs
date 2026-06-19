using Application.Features.DeliveryManSection.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Wallet.Queries
{
    public sealed record GetDeliveryManWalletTransactionsQuery : IRequest<Result<List<DeliveryManWalletTransactionDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 50;

        private class Handler : IRequestHandler<GetDeliveryManWalletTransactionsQuery, Result<List<DeliveryManWalletTransactionDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public Handler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<List<DeliveryManWalletTransactionDto>>> Handle(GetDeliveryManWalletTransactionsQuery request,
                                                                                    CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure<List<DeliveryManWalletTransactionDto>>("Delivery man not found");
                }

                var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
                var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

                var transactions = await context.DeliveryManWalletTransactions
                    .Where(x => x.DeliveryManId == deliveryManId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new DeliveryManWalletTransactionDto
                    {
                        Id = x.Id,
                        Amount = x.Amount,
                        Type = x.IsCredit ? "credit" : "debit",
                        TransactionType = x.IsCredit ? "credit" : "debit",
                        Description = x.Description,
                        DescriptionAr = x.DescriptionAr,
                        Status = x.Status,
                        OrderId = x.OrderId,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(transactions);
            }
        }
    }
}
