using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class DeliveryManLocation
    {
        public int Id { get;private set; }
        public int DeliveryManId { get;private set; }
        public double Longitude { get;private set; }
        public double Latitude { get;private set; }

        public static DeliveryManLocation Instsance(
                                                    double longitude,
                                                    double latitude)
        {
            return new DeliveryManLocation
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        public void UpdateLocation(double longitude,
                                   double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
    }
}
