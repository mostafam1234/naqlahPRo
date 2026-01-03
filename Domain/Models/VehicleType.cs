using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class VehicleType
    {
        public VehicleType()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this.VehicleTypeCategoies = new List<VehiclTypeCategory>();
        }
        public int Id { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }
        public decimal Cost { get;private set; }
        public string IconImagePath { get;private set; }
        private List<VehiclTypeCategory> _VehicleTypeCategoies { get; set; }
        public IReadOnlyList<VehiclTypeCategory> VehicleTypeCategoies
        {
            get
            {
                return _VehicleTypeCategoies;
            }
            private set
            {
                _VehicleTypeCategoies = (List<VehiclTypeCategory>)value.ToList();
            }
        }

        public static Result<VehicleType> Instance(string arabicName, string englishName, string iconImagePath, List<int> mainCategoryIds, decimal cost)
        {
            if (string.IsNullOrWhiteSpace(arabicName) || string.IsNullOrWhiteSpace(englishName))
            {
                return Result.Failure<VehicleType>("Invalid Vehicle Type Name");
            }

            if (string.IsNullOrWhiteSpace(iconImagePath))
            {
                return Result.Failure<VehicleType>("Icon is required");
            }

            if (mainCategoryIds == null || !mainCategoryIds.Any())
            {
                return Result.Failure<VehicleType>("At least one main category is required");
            }

            if (cost < 0)
            {
                return Result.Failure<VehicleType>("Cost cannot be negative");
            }
            
            var vehicleType = new VehicleType()
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                IconImagePath = iconImagePath,
                Cost = cost,
                _VehicleTypeCategoies = new List<VehiclTypeCategory>()
            };

            // Add main categories
            foreach (var categoryId in mainCategoryIds)
            {
                vehicleType._VehicleTypeCategoies.Add(new VehiclTypeCategory
                {
                    VehicleType = vehicleType,
                    MainCategoryId = categoryId
                });
            }

            return Result.Success(vehicleType);
        }

        public void Update(string arabicName, string englishName)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
        }

        public Result Update(string arabicName, string englishName, string iconImagePath, List<int> mainCategoryIds, decimal cost)
        {
            if (string.IsNullOrWhiteSpace(arabicName) || string.IsNullOrWhiteSpace(englishName))
            {
                return Result.Failure("Invalid Vehicle Type Name");
            }

            if (string.IsNullOrWhiteSpace(iconImagePath))
            {
                return Result.Failure("Icon is required");
            }

            if (mainCategoryIds == null || !mainCategoryIds.Any())
            {
                return Result.Failure("At least one main category is required");
            }

            if (cost < 0)
            {
                return Result.Failure("Cost cannot be negative");
            }

            ArabicName = arabicName;
            EnglishName = englishName;
            IconImagePath = iconImagePath;
            Cost = cost;

            // Clear existing categories
            _VehicleTypeCategoies.Clear();

            // Add new categories
            foreach (var categoryId in mainCategoryIds)
            {
                _VehicleTypeCategoies.Add(new VehiclTypeCategory
                {
                    VehicleType = this,
                    VehicleTypeId = this.Id,
                    MainCategoryId = categoryId
                });
            }

            return Result.Success();
        }
    }
}
