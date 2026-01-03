using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class City
    {
        private City()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this._Neighborhoods = new List<Neighborhood>();
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public int RegionId { get;private set; }
        public string EnglishName { get; private set; }
        private List<Neighborhood> _Neighborhoods { get; set; }
        public IReadOnlyList<Neighborhood> Neighborhoods
        {
            get
            {
                return _Neighborhoods;
            }
            private set
            {
                _Neighborhoods = (List<Neighborhood>)value.ToList();
            }
        }

        public static Result<City> Instance(string arabicName, string englishName, int regionId)
        {
            return new City()
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                RegionId = regionId
            };
        }

        public void Update(string arabicName, string englishName, int regionId)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
            RegionId = regionId;
        }
    }
}
