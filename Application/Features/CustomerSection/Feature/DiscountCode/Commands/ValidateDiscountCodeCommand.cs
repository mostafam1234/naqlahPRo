using Application.Features.CustomerSection.Feature.DiscountCode.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CustomerSection.Feature.DiscountCode.Commands
{
    public sealed record ValidateDiscountCodeCommand : IRequest<Result<ValidateDiscountCodeResponseDto>>
    {
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }

        private class Handler : IRequestHandler<ValidateDiscountCodeCommand, Result<ValidateDiscountCodeResponseDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<ValidateDiscountCodeResponseDto>> Handle(ValidateDiscountCodeCommand command, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(command.Code))
                    return Result.Failure<ValidateDiscountCodeResponseDto>("Code is required.");

                var discount = await _context.DiscountCodes
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Code == command.Code.Trim().ToUpper(), cancellationToken);

                if (discount == null)
                    return Result.Success(new ValidateDiscountCodeResponseDto
                    {
                        IsValid = false,
                        DiscountAmount = 0,
                        Message = "Invalid discount code."
                    });

                if (!discount.IsActive)
                    return Result.Success(new ValidateDiscountCodeResponseDto
                    {
                        IsValid = false,
                        DiscountAmount = 0,
                        Message = "This discount code is no longer active."
                    });

                if (discount.ExpiryDate.HasValue && discount.ExpiryDate.Value < DateTime.UtcNow)
                    return Result.Success(new ValidateDiscountCodeResponseDto
                    {
                        IsValid = false,
                        DiscountAmount = 0,
                        Message = "This discount code has expired."
                    });

                if (discount.UsageLimit.HasValue && discount.UsageCount >= discount.UsageLimit.Value)
                    return Result.Success(new ValidateDiscountCodeResponseDto
                    {
                        IsValid = false,
                        DiscountAmount = 0,
                        Message = "This discount code has reached its usage limit."
                    });

                return Result.Success(new ValidateDiscountCodeResponseDto
                {
                    IsValid = true,
                    DiscountAmount = discount.DiscountAmount,
                    Message = "Discount applied successfully."
                });
            }
        }
    }
}
