using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminSection.DiscountCodeFeatures.Commands
{
    public sealed record UpdateDiscountCodeCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public bool? IsActive { get; set; }

        private class Handler : IRequestHandler<UpdateDiscountCodeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(UpdateDiscountCodeCommand command, CancellationToken cancellationToken)
            {
                var discount = await _context.DiscountCodes
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

                if (discount == null)
                    return Result.Failure<int>("Discount code not found.");

                if (!string.IsNullOrWhiteSpace(command.Code))
                {
                    var codeExists = _context.DiscountCodes.Any(x =>
                        x.Code == command.Code.Trim().ToUpper() && x.Id != command.Id);
                    if (codeExists)
                        return Result.Failure<int>("A discount code with this value already exists.");
                }

                discount.Update(
                    command.Code,
                    command.DiscountAmount,
                    command.UsageLimit,
                    command.ExpiryDate,
                    command.MinimumOrderAmount,
                    command.IsActive);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<int>(saveResult.Error);

                return Result.Success(discount.Id);
            }
        }
    }
}
