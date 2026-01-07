using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public sealed record AddOrderPackageCommand : IRequest<Result<int>>
    {
        public string ArabicDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;
        public decimal MinWeightInKiloGram { get; set; }
        public decimal MaxWeightInKiloGram { get; set; }

        private class AddOrderPackageCommandHandler : IRequestHandler<AddOrderPackageCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public AddOrderPackageCommandHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(AddOrderPackageCommand command, CancellationToken cancellationToken)
            {
                var orderPackage = OrderPackage.Instance(
                    command.ArabicDescription,
                    command.EnglishDescription,
                    command.MinWeightInKiloGram,
                    command.MaxWeightInKiloGram);

                if (orderPackage.IsFailure)
                {
                    return Result.Failure<int>(orderPackage.Error);
                }

                await _context.OrderPackages.AddAsync(orderPackage.Value, cancellationToken);
                var result = await _context.SaveChangesAsyncWithResult();

                if (result.IsSuccess)
                {
                    return Result.Success(orderPackage.Value.Id);
                }

                return Result.Failure<int>(result.Error);
            }
        }
    }
}

