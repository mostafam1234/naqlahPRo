using Domain.Enums;

namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public sealed class DeliveryManLookupDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool Active { get; set; }
        public string ActiveStatusName { get; set; } = string.Empty;
        public DeliveryRequesState DeliveryState { get; set; }
        public string DeliveryStateName { get; set; } = string.Empty;
    }
}
