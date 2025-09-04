using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.OrderPack.Dtos
{
    public class OrderPackDto
    {
        public OrderPackDto()
        {
            this.Description = string.Empty;
        }
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
