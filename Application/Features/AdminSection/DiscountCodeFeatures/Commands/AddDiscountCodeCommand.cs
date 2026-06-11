using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;

namespace Application.Features.AdminSection.DiscountCodeFeatures.Commands
{
    public sealed record AddDiscountCodeCommand : IRequest<Result<int>>
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? MinimumOrderAmount { get; set; }

        private class Handler : IRequestHandler<AddDiscountCodeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(AddDiscountCodeCommand command, CancellationToken cancellationToken)
            {
                var exists = _context.DiscountCodes.Any(x => x.Code == command.Code.Trim().ToUpper());
                if (exists)
                    return Result.Failure<int>("A discount code with this value already exists.");

                var discountResult = DiscountCode.Instance(
                    command.Code,
                    command.DiscountAmount,
                    command.UsageLimit,
                    command.ExpiryDate,
                    command.MinimumOrderAmount);

                if (discountResult.IsFailure)
                    return Result.Failure<int>(discountResult.Error);

                var discount = discountResult.Value;
                await _context.DiscountCodes.AddAsync(discount, cancellationToken);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure<int>(saveResult.Error);

                return Result.Success(discount.Id);
            }
        }
    }
}
