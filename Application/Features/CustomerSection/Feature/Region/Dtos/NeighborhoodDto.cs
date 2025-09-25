using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Region.Dtos
{
    public class NeighborhoodDto
    {
        public NeighborhoodDto()
        {
            this.Name = string.Empty;
        }
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Name { get; set; }
    }
}
