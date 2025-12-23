using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class GetAllOrdersDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public OrderType OrderType { get; set; }
        public string OrderTypeName { get; set; } = string.Empty;
        public decimal Total { get; set; }

        // Customer Information
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public CustomerType CustomerType { get; set; }
        public string CustomerTypeName { get; set; } = string.Empty;

        // Delivery Man Information
        public int? DeliveryManId { get; set; }
        public string DeliveryManName { get; set; } = string.Empty;
        public string DeliveryManPhone { get; set; } = string.Empty;

        // Additional Info
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // WayPoints Information
        public List<OrderWayPointAdminDto> WayPoints { get; set; } = new List<OrderWayPointAdminDto>();
    }


}
