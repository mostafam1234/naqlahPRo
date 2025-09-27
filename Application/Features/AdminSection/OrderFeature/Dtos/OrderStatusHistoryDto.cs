using Domain.Enums;
using System;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class OrderStatusHistoryDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}