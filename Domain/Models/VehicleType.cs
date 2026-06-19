using CSharpFunctionalExtensions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Models
{
    public class VehicleType
    {
        public VehicleType()
        {
            ArabicName = string.Empty;
            EnglishName = string.Empty;
            IconImagePath = string.Empty;
            _VehicleTypeCategoies = new List<VehiclTypeCategory>();
            CreationDate = DateTime.UtcNow;
        }

        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        public decimal Cost { get; private set; }
        public decimal ServiceFee { get; private set; }
        public string IconImagePath { get; private set; }
        public VehicleLoadCategory? LoadCategory { get; private set; }
        public DateTime CreationDate { get; private set; }

        private List<VehiclTypeCategory> _VehicleTypeCategoies { get; set; }

        public IReadOnlyList<VehiclTypeCategory> VehicleTypeCategoies
        {
            get => _VehicleTypeCategoies;
            private set => _VehicleTypeCategoies = value.ToList();
        }

        public static Result<VehicleType> Instance(
            string arabicName,
            string englishName,
            string iconImagePath,
            List<int>? mainCategoryIds,
            decimal cost,
            decimal serviceFee,
            VehicleLoadCategory? loadCategory)
        {
            if (cost < 0)
                return Result.Failure<VehicleType>("CostCannotBeNegative");

            if (serviceFee < 0)
                return Result.Failure<VehicleType>("ServiceFeeCannotBeNegative");

            var vehicleType = new VehicleType
            {
                ArabicName = arabicName?.Trim() ?? string.Empty,
                EnglishName = englishName?.Trim() ?? string.Empty,
                IconImagePath = iconImagePath?.Trim() ?? string.Empty,
                Cost = cost,
                ServiceFee = serviceFee,
                LoadCategory = loadCategory,
                CreationDate = DateTime.UtcNow,
                _VehicleTypeCategoies = new List<VehiclTypeCategory>()
            };

            if (mainCategoryIds != null)
            {
                foreach (var categoryId in mainCategoryIds.Distinct().Where(id => id > 0))
                {
                    vehicleType._VehicleTypeCategoies.Add(new VehiclTypeCategory
                    {
                        VehicleType = vehicleType,
                        MainCategoryId = categoryId
                    });
                }
            }

            return Result.Success(vehicleType);
        }

        public void Update(string arabicName, string englishName)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
        }

        public Result Update(
            string arabicName,
            string englishName,
            string iconImagePath,
            List<int>? mainCategoryIds,
            decimal cost,
            decimal serviceFee,
            VehicleLoadCategory? loadCategory)
        {
            if (cost < 0)
                return Result.Failure("CostCannotBeNegative");

            if (serviceFee < 0)
                return Result.Failure("ServiceFeeCannotBeNegative");

            ArabicName = arabicName?.Trim() ?? string.Empty;
            EnglishName = englishName?.Trim() ?? string.Empty;
            IconImagePath = iconImagePath?.Trim() ?? string.Empty;
            Cost = cost;
            ServiceFee = serviceFee;
            LoadCategory = loadCategory;

            _VehicleTypeCategoies.Clear();

            if (mainCategoryIds != null)
            {
                foreach (var categoryId in mainCategoryIds.Distinct().Where(id => id > 0))
                {
                    _VehicleTypeCategoies.Add(new VehiclTypeCategory
                    {
                        VehicleType = this,
                        VehicleTypeId = Id,
                        MainCategoryId = categoryId
                    });
                }
            }

            return Result.Success();
        }
    }
}
