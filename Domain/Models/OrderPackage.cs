using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Domain.Models
{
    public class OrderPackage
    {
        private OrderPackage()
        {
            this.ArabicDescripton = string.Empty;
            this.EnglishDescription = string.Empty;
        }
        public int Id { get;private set; }
        public string ArabicDescripton { get;private set; }
        public string EnglishDescription { get;private set; }
        public decimal MinWeightInKiloGram { get;private set; }
        public decimal MaxWeightInKiloGram { get;private set; }

        public static Result<OrderPackage> Instance(
            string arabicDescription,
            string englishDescription,
            decimal minWeightInKiloGram,
            decimal maxWeightInKiloGram)
        {
            if (string.IsNullOrWhiteSpace(arabicDescription))
                return Result.Failure<OrderPackage>("Arabic description is required.");
            if (string.IsNullOrWhiteSpace(englishDescription))
                return Result.Failure<OrderPackage>("English description is required.");
            if (minWeightInKiloGram < 0)
                return Result.Failure<OrderPackage>("Minimum weight cannot be negative.");
            if (maxWeightInKiloGram < 0)
                return Result.Failure<OrderPackage>("Maximum weight cannot be negative.");
            if (minWeightInKiloGram > maxWeightInKiloGram)
                return Result.Failure<OrderPackage>("Minimum weight cannot be greater than maximum weight.");

            return new OrderPackage
            {
                ArabicDescripton = arabicDescription,
                EnglishDescription = englishDescription,
                MinWeightInKiloGram = minWeightInKiloGram,
                MaxWeightInKiloGram = maxWeightInKiloGram
            };
        }

        public void Update(
            string arabicDescription,
            string englishDescription,
            decimal minWeightInKiloGram,
            decimal maxWeightInKiloGram)
        {
            if (!string.IsNullOrWhiteSpace(arabicDescription))
                ArabicDescripton = arabicDescription;
            if (!string.IsNullOrWhiteSpace(englishDescription))
                EnglishDescription = englishDescription;
            if (minWeightInKiloGram >= 0)
                MinWeightInKiloGram = minWeightInKiloGram;
            if (maxWeightInKiloGram >= 0)
                MaxWeightInKiloGram = maxWeightInKiloGram;
        }
    }
}
