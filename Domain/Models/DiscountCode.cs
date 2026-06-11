using CSharpFunctionalExtensions;

namespace Domain.Models
{
    public class DiscountCode
    {
        private DiscountCode()
        {
            Code = string.Empty;
        }

        public int Id { get; private set; }
        public string Code { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public bool IsActive { get; private set; }
        public int? UsageLimit { get; private set; }
        public int UsageCount { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public decimal? MinimumOrderAmount { get; private set; }
        public DateTime CreatedDate { get; private set; }

        public static Result<DiscountCode> Instance(
            string code,
            decimal discountAmount,
            int? usageLimit = null,
            DateTime? expiryDate = null,
            decimal? minimumOrderAmount = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure<DiscountCode>("Code is required.");
            if (discountAmount <= 0)
                return Result.Failure<DiscountCode>("Discount amount must be greater than zero.");
            if (usageLimit.HasValue && usageLimit.Value <= 0)
                return Result.Failure<DiscountCode>("Usage limit must be greater than zero.");
            if (minimumOrderAmount.HasValue && minimumOrderAmount.Value < 0)
                return Result.Failure<DiscountCode>("Minimum order amount cannot be negative.");

            return new DiscountCode
            {
                Code = code.Trim().ToUpper(),
                DiscountAmount = discountAmount,
                IsActive = true,
                UsageLimit = usageLimit,
                UsageCount = 0,
                ExpiryDate = expiryDate,
                MinimumOrderAmount = minimumOrderAmount,
                CreatedDate = DateTime.UtcNow
            };
        }

        public void Update(
            string? code,
            decimal? discountAmount,
            int? usageLimit,
            DateTime? expiryDate,
            decimal? minimumOrderAmount,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(code))
                Code = code.Trim().ToUpper();
            if (discountAmount.HasValue && discountAmount.Value > 0)
                DiscountAmount = discountAmount.Value;
            if (usageLimit.HasValue)
                UsageLimit = usageLimit;
            if (expiryDate.HasValue)
                ExpiryDate = expiryDate;
            if (minimumOrderAmount.HasValue)
                MinimumOrderAmount = minimumOrderAmount;
            if (isActive.HasValue)
                IsActive = isActive.Value;
        }

        public Result IncrementUsage()
        {
            if (!IsActive)
                return Result.Failure("Discount code is not active.");
            if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value)
                return Result.Failure("Discount code usage limit has been reached.");
            UsageCount++;
            return Result.Success();
        }

        public void Deactivate() => IsActive = false;
    }
}
