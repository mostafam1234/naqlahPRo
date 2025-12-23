using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SystemConfiguration
    {
        public int Id { get;private set; }
        public int BaseKm { get;private set; }
        public decimal BaseKmRate { get;private set; }
        public decimal ExtraKmRate { get;private set; }
        public int BaseHours { get;private set; }      
        public decimal BaseHourRate { get;private set; }
        public decimal ExtraHourRate { get;private set; }
        public decimal VatRate { get;private set; }
        public decimal ServiceFess { get;private set; }

        public void Update(int baseKm,
                            decimal baseKmRate,
                            decimal extraKmRate,
                            int baseHour,
                            decimal baseHourRate,
                            decimal extraHourRate,
                            decimal vatRate,
                            decimal servicefees)
        {
            this.BaseKm = baseKm;
            this.BaseKmRate = baseKmRate;
            this.ExtraKmRate = extraKmRate;
            this.BaseHours = baseHour;
            this.BaseHourRate = baseHourRate;
            this.ExtraHourRate = extraHourRate;
            this.VatRate = vatRate;
            this.ServiceFess = servicefees;
        }
    }
}
