namespace Application.Features.AdminSection.DiscountCodeFeatures.Dtos
{
    public class DiscountCodeAdminDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
