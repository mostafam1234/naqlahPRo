using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class VehicleBrand
    {
        public VehicleBrand()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }

        public static Result<VehicleBrand> Instance(string arabicName, string englishName)
        {
            if (string.IsNullOrWhiteSpace(arabicName) || string.IsNullOrWhiteSpace(englishName))
            {
                return Result.Failure<VehicleBrand>("Invalid Vehicle Brand Name");
            }

            return Result.Success(new VehicleBrand()
            {
                ArabicName = arabicName,
                EnglishName = englishName
            });
        }

        public void Update(string arabicName, string englishName)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
        }
    }
}
