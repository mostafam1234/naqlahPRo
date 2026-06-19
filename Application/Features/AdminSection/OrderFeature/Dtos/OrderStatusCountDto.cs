using Domain.Enums;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public sealed class OrderStatusCountDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
