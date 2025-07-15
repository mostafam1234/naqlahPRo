using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class DeliveryManTokenResponse
    {
        public TokenResponse TokenResponse { get; set; }
        public bool RequiredDeliveryInfo { get; set; }
        public bool RequiredPersonalInfo { get; set; }
        public bool RequiredVehicleInfo { get; set; }
        public int? CarOwnerType { get; set; }
    }
}
