using Domain.Enums;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class OrderWayPointAdminDto
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsOrigin { get; set; }
        public bool IsDestination { get; set; }
        public OrderWayPointsStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public System.DateTime? PickedUpDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string NeighborhoodName { get; set; } = string.Empty;
    }
}