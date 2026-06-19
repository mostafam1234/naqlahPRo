namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public class SetDeliveryManActiveStatusResultDto
    {
        public int DeliveryManId { get; set; }
        public bool Active { get; set; }
        public string ActiveStatusName { get; set; } = string.Empty;
        public bool StatusChanged { get; set; }
    }
}
