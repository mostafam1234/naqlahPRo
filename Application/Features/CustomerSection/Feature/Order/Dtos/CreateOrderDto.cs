using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class CreateOrderDto
    {
        public CreateOrderDto()
        {
            this.MainCategoryIds = new List<int>();
            this.WayPoints = new List<CreateWayPointsDto>();
            this.OrderServiceIds = new List<int>();
        }
        public int OrderPackId { get; set; }
        public int OrderTypeId { get; set; }
        public int VehicleTypdId { get; set; }
        public List<int> MainCategoryIds { get; set; }
        public List<int> OrderServiceIds { get; set; }
        public List<CreateWayPointsDto> WayPoints { get; set; }
    }
}
