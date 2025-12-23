using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemConfiguration.Dto
{
    public class SystemConfigurationDto
    {
        public int Id { get;  set; }
        public int BaseKm { get;  set; }
        public decimal BaseKmRate { get;  set; }
        public decimal ExtraKmRate { get;  set; }
        public int BaseHours { get;  set; }
        public decimal BaseHourRate { get;  set; }
        public decimal ExtraHourRate { get;  set; }
        public decimal VatRate { get;  set; }
        public decimal ServiceFess { get;  set; }
    }
}
