using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public sealed record DeleteOrderPackageCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        private class DeleteOrderPackageCommandHandler : IRequestHandler<DeleteOrderPackageCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public DeleteOrderPackageCommandHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(DeleteOrderPackageCommand command, CancellationToken cancellationToken)
            {
                var orderPackage = await _context.OrderPackages.AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (orderPackage == null)
                {
                    return Result.Failure<int>("OrderPackageNotFound");
                }

                // Check if any orders are using this package
                var hasOrders = await _context.Orders.AnyAsync(x => x.OrderPackageId == command.Id, cancellationToken);
                if (hasOrders)
                {
                    return Result.Failure<int>("OrderPackageInUseCannotDelete");
                }

                await _context.OrderPackages.Where(x => x.Id == command.Id).ExecuteDeleteAsync(cancellationToken);
                var result = await _context.SaveChangesAsyncWithResult();

                if (result.IsSuccess)
                {
                    return Result.Success(orderPackage.Id);
                }

                return Result.Failure<int>(result.Error);
            }
        }
    }
}

