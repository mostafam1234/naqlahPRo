using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AssistanWork
    {
        private AssistanWork()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        public decimal Cost { get; private set; }
        public bool IsDeleted { get; private set; }

        public static Result<AssistanWork> Instance(string arabicName, string englishName, decimal cost)
        {
            return new AssistanWork()
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                Cost = cost,
                IsDeleted = false
            };
        }

        public void Update(string arabicName, string englishName, decimal cost)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
            Cost = cost;
        }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
