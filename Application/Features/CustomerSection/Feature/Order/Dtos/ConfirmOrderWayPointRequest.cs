using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class ChangeOrderWayPointStatusRequest
    {
        public int OrderId { get; set; }
        public int OrderWayPointId { get; set; }
    }
}
