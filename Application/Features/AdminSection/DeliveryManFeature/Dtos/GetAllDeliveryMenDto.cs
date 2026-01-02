namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public class GetAllDeliveryMenDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PersonalImagePath { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string DeliveryTypeName { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}

