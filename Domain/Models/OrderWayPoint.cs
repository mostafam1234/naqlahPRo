using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderWayPoint
    {
        public OrderWayPoint()
        {
            this.PackImagePath = string.Empty;
        }
        public int Id { get; private set; }
        public double Latitude { get; private set; }
        public double longitude { get; private set; }
        public int OrderId { get; private set; }
        public int RegionId { get; private set; }
        public int CityId { get; private set; }
        public int NeighborhoodId { get; private set; }
        public DateTime? PickedUpDate { get;private set; }
        public OrderWayPointsStatus OrderWayPointsStatus { get;private set; }
        public string PackImagePath { get;private set; }
        public bool IsOrgin { get; private set; }
        public bool IsDestination { get; private set; }
        public Region Region { get; private set; }
        public City City { get; private set; }
        public Neighborhood Neighborhood { get; private set; }

        public static OrderWayPoint Create(double latitude,
                                           double longitude,
                                           int regionId,
                                           int cityId,
                                           int neighborhoodId,
                                           bool isOrgin,
                                           bool isDestination)
        {
            return new OrderWayPoint
            {
                Latitude = latitude,
                longitude = longitude,
                RegionId = regionId,
                CityId = cityId,
                NeighborhoodId = neighborhoodId,
                IsOrgin = isOrgin,
                IsDestination = isDestination,
                OrderWayPointsStatus = OrderWayPointsStatus.Pending
            };
        }
    }
}
