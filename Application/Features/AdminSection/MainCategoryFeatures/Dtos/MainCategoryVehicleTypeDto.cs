using Domain.Enums;

namespace Application.Features.AdminSection.MainCategoryFeatures.Dtos
{
    public sealed class MainCategoryVehicleTypeDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public VehicleLoadCategory? LoadCategory { get; set; }
        public string LoadCategoryArabicName { get; set; } = string.Empty;
        public string LoadCategoryEnglishName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
    }
}
