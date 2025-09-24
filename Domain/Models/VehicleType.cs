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
        }
        public int Id { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }

        public static Result<VehicleType> Instance(string arabicName, string englishName)
        {
            if (string.IsNullOrWhiteSpace(arabicName) || string.IsNullOrWhiteSpace(englishName))
            {
                return Result.Failure<VehicleType>("Invalid Vehicle Type Name");
            }
            return Result.Success(new VehicleType()
            {
                ArabicName = arabicName,
                EnglishName = englishName
            });
        }
    }
}
