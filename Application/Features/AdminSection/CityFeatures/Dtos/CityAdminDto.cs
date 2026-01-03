using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CityFeatures.Dtos
{
    public class CityAdminDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
    }
}


