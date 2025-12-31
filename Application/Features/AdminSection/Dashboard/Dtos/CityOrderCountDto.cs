using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Dtos
{
    public class CityOrderCountDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
    }
}

