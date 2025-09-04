using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.VehicleType.Dtos
{
    public class VehicleTypeDto
    {
        public VehicleTypeDto()
        {
            this.Name = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
