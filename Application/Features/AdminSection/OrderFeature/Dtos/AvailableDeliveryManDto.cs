namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class AvailableDeliveryManDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
    }
}

