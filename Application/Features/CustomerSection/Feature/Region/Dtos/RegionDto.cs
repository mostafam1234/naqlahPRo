using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Region.Dtos
{
    public class RegionDto
    {
        public RegionDto()
        {
            this.Name = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CityDto> Cities { get; set; } = new List<CityDto>();
    }
}
