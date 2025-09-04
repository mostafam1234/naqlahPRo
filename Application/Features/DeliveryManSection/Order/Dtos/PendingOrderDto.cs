using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Order.Dtos
{
    public class PendingOrderDto
    {
        public PendingOrderDto()
        {
            this.OrderNumber = string.Empty;
            this.Categories = new List<string>();
            this.PickupAddress = string.Empty;
            this.DeliveryAddress = string.Empty;
        }
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal Total { get; set; }
        public double Distance { get; set; } // Distance in kilometers from delivery man
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DeliveryLatitude { get; set; }
        public double DeliveryLongitude { get; set; }
        public string PickupAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public List<string> Categories { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}