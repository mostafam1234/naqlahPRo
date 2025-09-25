using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class SelectVehicleTypeDto
    {
        public int OrderId { get; set; }
        public int VehicleTypeId { get; set; }
    }

    public class SelectVehicleTypeResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NotifiedDeliveryMenCount { get; set; }
    }
}