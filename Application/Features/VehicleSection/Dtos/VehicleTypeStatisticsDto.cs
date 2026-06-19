using System.Collections.Generic;

namespace Application.Features.VehicleSection.Dtos
{
    public sealed class VehicleTypeStatisticsDto
    {
        public int TotalVehicleTypes { get; set; }
        public int TotalRegisteredVehicles { get; set; }
        public List<VehicleLoadCategoryCountDto> LoadCategoryCounts { get; set; } = new();
        public List<MainCategoryVehicleCountDto> MainCategoryCounts { get; set; } = new();
    }

    public sealed class VehicleLoadCategoryCountDto
    {
        public int? LoadCategory { get; set; }
        public string LoadCategoryName { get; set; } = string.Empty;
        public int VehicleTypeCount { get; set; }
        public int RegisteredVehicleCount { get; set; }
    }

    public sealed class MainCategoryVehicleCountDto
    {
        public int MainCategoryId { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int VehicleTypeCount { get; set; }
        public int RegisteredVehicleCount { get; set; }
    }
}
