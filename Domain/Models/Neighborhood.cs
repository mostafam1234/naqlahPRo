using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Neighborhood
    {
        private Neighborhood()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public int CityId { get;private set; }
        public string EnglishName { get; private set; }

        public static Result<Neighborhood> Instance(string arabicName, string englishName, int cityId)
        {
            return new Neighborhood()
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                CityId = cityId
            };
        }

        public void Update(string arabicName, string englishName, int cityId)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
            CityId = cityId;
        }
    }
}
