using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;

namespace Application.Features.AdminSection.AdditionalService.Commands
{
    public sealed record AddAdditionalServiceCommand : IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        private class AddAdditionalServiceCommandHandler : IRequestHandler<AddAdditionalServiceCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public AddAdditionalServiceCommandHandler(INaqlahContext context) => _context = context;

            public async Task<Result<int>> Handle(AddAdditionalServiceCommand command, CancellationToken cancellationToken)
            {
                var entity = Domain.Models.AdditionalService.Create(command.ArabicName,
                                                                    command.EnglishName,
                                                                    command.UnitPrice);
                await _context.AdditionalServices.AddAsync(entity, cancellationToken);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return Result.Success(entity.Id);
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
