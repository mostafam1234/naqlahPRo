using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AdditionalService
    {
        private AdditionalService()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }

        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public bool IsDeleted { get; private set; }

        public static AdditionalService Create(string arabicName, string englishName, decimal unitPrice)
        {
            return new AdditionalService
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                UnitPrice = unitPrice,
                IsDeleted = false
            };
        }

        public void Update(string arabicName, string englishName, decimal unitPrice)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
            UnitPrice = unitPrice;
        }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
