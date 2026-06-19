using Application.Features.DeliveryManSection.Wallet.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Wallet.Commands
{
    public sealed record RequestDeliveryManWithdrawalCommand : IRequest<Result<DeliveryManWithdrawResponseDto>>
    {
        public decimal Amount { get; init; }
        public int BankAccountId { get; init; }

        private class Handler : IRequestHandler<RequestDeliveryManWithdrawalCommand, Result<DeliveryManWithdrawResponseDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IDateTimeProvider dateTimeProvider;

            public Handler(INaqlahContext context, IUserSession userSession, IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.userSession = userSession;
                this.dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result<DeliveryManWithdrawResponseDto>> Handle(RequestDeliveryManWithdrawalCommand request,
                                                                             CancellationToken cancellationToken)
            {
                if (request.Amount <= 0)
                {
                    return Result.Failure<DeliveryManWithdrawResponseDto>("Amount must be greater than zero");
                }

                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure<DeliveryManWithdrawResponseDto>("Delivery man not found");
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

                var available = balance - pendingBalance;
                if (available < request.Amount)
                {
                    return Result.Failure<DeliveryManWithdrawResponseDto>(
                        $"Insufficient balance. Available: {available:F2}, Requested: {request.Amount:F2}");
                }

                var transactionResult = DeliveryManWalletTransaction.Create(
                    deliveryManId,
                    request.Amount,
                    isCredit: false,
                    description: "Withdrawal request",
                    descriptionAr: "طلب سحب رصيد",
                    status: DeliveryManWalletTransaction.StatusPending,
                    createdAt: dateTimeProvider.Now,
                    orderId: null,
                    bankAccountId: request.BankAccountId);

                if (transactionResult.IsFailure)
                {
                    return Result.Failure<DeliveryManWithdrawResponseDto>(transactionResult.Error);
                }

                await context.DeliveryManWalletTransactions.AddAsync(transactionResult.Value, cancellationToken);

                var saveResult = await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<DeliveryManWithdrawResponseDto>(saveResult.Error);
                }

                return Result.Success(new DeliveryManWithdrawResponseDto
                {
                    Success = true,
                    Message = "Withdrawal request submitted",
                    Balance = balance,
                    PendingBalance = pendingBalance + request.Amount
                });
            }
        }
    }
}
