using Domain.Enums;

namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public sealed class DeliveryManOrderStatusCountDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
