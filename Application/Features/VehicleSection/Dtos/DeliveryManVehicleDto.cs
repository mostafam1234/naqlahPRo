using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.VehicleSection.Dtos
{
    public class DeliveryManVehicleDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string IconImagePath { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal ServiceFee { get; set; }
        public VehicleLoadCategory? LoadCategory { get; set; }
        public string? LoadCategoryName { get; set; }
        public DateTime CreationDate { get; set; }
        public List<MainCategoryInfo> MainCategories { get; set; } = new List<MainCategoryInfo>();
    }

    public class MainCategoryInfo
    {
        public int Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
