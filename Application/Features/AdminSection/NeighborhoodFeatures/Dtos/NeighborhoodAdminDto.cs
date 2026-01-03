using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NeighborhoodFeatures.Dtos
{
    public class NeighborhoodAdminDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
    }
}


