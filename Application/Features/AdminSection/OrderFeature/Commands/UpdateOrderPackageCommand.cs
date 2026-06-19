using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public sealed record UpdateOrderPackageCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;
        public decimal MinWeightInKiloGram { get; set; }
        public decimal MaxWeightInKiloGram { get; set; }

        private class UpdateOrderPackageCommandHandler : IRequestHandler<UpdateOrderPackageCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public UpdateOrderPackageCommandHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(UpdateOrderPackageCommand command, CancellationToken cancellationToken)
            {
                var orderPackage = await _context.OrderPackages.AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (orderPackage == null)
                {
                    return Result.Failure<int>("OrderPackageNotFound");
                }

                // Validate before updating
                if (string.IsNullOrWhiteSpace(command.ArabicDescription))
                    return Result.Failure<int>("ArabicDescriptionRequired");
                if (string.IsNullOrWhiteSpace(command.EnglishDescription))
                    return Result.Failure<int>("EnglishDescriptionRequired");
                if (command.MinWeightInKiloGram < 0)
                    return Result.Failure<int>("MinimumWeightCannotBeNegative");
                if (command.MaxWeightInKiloGram < 0)
                    return Result.Failure<int>("MaximumWeightCannotBeNegative");
                if (command.MinWeightInKiloGram > command.MaxWeightInKiloGram)
                    return Result.Failure<int>("MinimumWeightGreaterThanMaximum");

                orderPackage.Update(
                    command.ArabicDescription,
                    command.EnglishDescription,
                    command.MinWeightInKiloGram,
                    command.MaxWeightInKiloGram);

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

