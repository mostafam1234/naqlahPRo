using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.DiscountCodeFeatures.Commands
{
    public sealed record DeleteDiscountCodeCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        private class Handler : IRequestHandler<DeleteDiscountCodeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(DeleteDiscountCodeCommand command, CancellationToken cancellationToken)
            {
                var discount = await _context.DiscountCodes
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (discount == null)
                    return Result.Failure<int>("Discount code not found.");

                await _context.DiscountCodes
                    .Where(x => x.Id == command.Id)
                    .ExecuteDeleteAsync(cancellationToken);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<int>(saveResult.Error);

                return Result.Success(discount.Id);
            }
        }
    }
}
