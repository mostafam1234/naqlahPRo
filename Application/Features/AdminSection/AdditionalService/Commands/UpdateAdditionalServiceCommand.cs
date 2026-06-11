using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.AdditionalService.Commands
{
    public sealed record UpdateAdditionalServiceCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        private class UpdateAdditionalServiceCommandHandler : IRequestHandler<UpdateAdditionalServiceCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateAdditionalServiceCommandHandler(INaqlahContext context) => _context = context;

            public async Task<Result<int>> Handle(UpdateAdditionalServiceCommand command, CancellationToken cancellationToken)
            {
                var entity = await _context.AdditionalServices.AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
                if (entity is null)
                    return Result.Failure<int>("Additional service not found");

                entity.Update(command.ArabicName, command.EnglishName, command.UnitPrice);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return Result.Success(entity.Id);
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
