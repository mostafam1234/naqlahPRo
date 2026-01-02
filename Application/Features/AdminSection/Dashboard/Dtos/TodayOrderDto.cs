using Domain.Enums;
using System;

namespace Application.Features.AdminSection.Dashboard.Dtos
{
    public class TodayOrderDto
    {
        public int Id { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public OrderType OrderType { get; set; }
        public string OrderTypeName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string OrderStatusName { get; set; } = string.Empty;
    }
}

