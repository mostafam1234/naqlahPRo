using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class CreateWayPointsDto
    {
        public double Latitude { get;  set; }
        public double Longitude { get;  set; }
        public int RegionId { get;  set; }
        public int CityId { get;  set; }
        public int NeighborhoodId { get;  set; }
        public bool IsOrgin { get; set; }
        public bool IsDestenation { get; set; }
    }
}
