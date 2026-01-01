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
            this.MatchingVehicles = new List<OrderVehicleDto>();
        }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public List<OrderVehicleDto> MatchingVehicles { get; set; }
    }

    public class OrderVehicleDto
    {
        public OrderVehicleDto()
        {
            this.Name = string.Empty;
            this.IconPath = string.Empty;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string IconPath { get; set; }
    }
}