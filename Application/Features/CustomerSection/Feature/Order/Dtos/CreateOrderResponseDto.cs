using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class CreateOrderResponseDto
    {
        public CreateOrderResponseDto()
        {
            this.MatchingVehicles = new List<VehicleDto>();
        }
        public int OrderId { get; set; }
        public List<VehicleDto> MatchingVehicles { get; set; }
    }

    public class VehicleDto
    {
        public VehicleDto()
        {
            this.Name = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}