using Domain.Enums;

namespace Application.Features.VehicleSection.Dtos
{
    public sealed class VehicleLoadCategoryLookupDto
    {
        public VehicleLoadCategory Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
