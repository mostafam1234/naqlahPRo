namespace Application.Features.CustomerSection.Feature.DiscountCode.Dtos
{
    public class ValidateDiscountCodeResponseDto
    {
        public bool IsValid { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
