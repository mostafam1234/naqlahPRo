namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class OrderPackageDto
    {
        public int Id { get; set; }
        public string ArabicDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinWeightInKg { get; set; }
        public decimal MaxWeightInKg { get; set; }
    }
}