using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.GoogleMap
{
    public class GoogleResponse
    {
        public GoogleResponse()
        {
            this.EncodedPoints = string.Empty;
        }
        public double DeliveryTime { get; set; }
        public string EncodedPoints { get; set; }
        public double DistanceInKiloMeter { get; set; }
    }
}
