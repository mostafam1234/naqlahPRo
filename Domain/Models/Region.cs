using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Region
    {
        public Region()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this._Cities = new List<City>();
        }
        public int Id { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get; private set; }
        private List<City> _Cities { get; set; }
        public IReadOnlyList<City> Cities
        {
            get
            {
                return _Cities;
            }
            private set
            {
                _Cities = (List<City>)value.ToList();
            }
        }
    }
}
