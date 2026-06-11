using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.AdditionalService.Commands
{
    public sealed record DeleteAdditionalServiceCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        private class DeleteAdditionalServiceCommandHandler : IRequestHandler<DeleteAdditionalServiceCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteAdditionalServiceCommandHandler(INaqlahContext context) => _context = context;

            public async Task<Result<int>> Handle(DeleteAdditionalServiceCommand command, CancellationToken cancellationToken)
            {
                var entity = await _context.AdditionalServices.AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
                if (entity is null)
                    return Result.Failure<int>("Additional service not found");

                entity.Delete();
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return Result.Success(entity.Id);
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
