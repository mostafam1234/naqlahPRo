using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.WalletTransactionFeatures.Commands
{
    public sealed record AddWalletTransactionCommand : IRequest<Result<int>>
    {
        public string ArabicDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public bool Withdraw { get; set; }
        public int? OrderId { get; set; }

        private class AddWalletTransactionCommandHandler : IRequestHandler<AddWalletTransactionCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IDateTimeProvider dateTimeProvider;
            public AddWalletTransactionCommandHandler(INaqlahContext context, IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                this.dateTimeProvider = dateTimeProvider;
            }
            public async Task<Result<int>> Handle(AddWalletTransactionCommand command, CancellationToken cancellationToken)
            {
                var transaction = WalletTransctions.Instance(
                    dateTimeProvider.Now,
                    command.ArabicDescription,
                    command.EnglishDescription,
                    command.CustomerId,
                    command.Amount,
                    command.Withdraw,
                    command.OrderId);

                if (transaction.IsFailure)
                {
                    return Result.Failure<int>(transaction.Error);
                }

                await _context.WalletTransctions.AddAsync(transaction.Value, cancellationToken);
                var result = await _context.SaveChangesAsyncWithResult();
                
                if (result.IsSuccess)
                {
                    return Result.Success(transaction.Value.Id);
                }
                
                return Result.Failure<int>(result.Error);
            }
        }
    }
}




