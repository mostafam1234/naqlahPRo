using System.Collections.Generic;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public sealed class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int ConfirmedGoingToPickupOrders { get; set; }
        public int PickedUpOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
    }
}
