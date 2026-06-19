namespace Application.Features.AdminSection.MainCategoryFeatures.Dtos
{
    public sealed class MainCategoryVehicleTypesResultDto
    {
        public int MainCategoryId { get; set; }
        public string MainCategoryArabicName { get; set; } = string.Empty;
        public string MainCategoryEnglishName { get; set; } = string.Empty;
        public List<MainCategoryVehicleTypeDto> VehicleTypes { get; set; } = new();
    }
}
