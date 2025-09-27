using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class GetOrderDetailsForAdminDto
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

        // Vehicle Information
        public int? VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;

        // Route Information
        public List<OrderWayPointAdminDto> WayPoints { get; set; } = new();

        // Package Information
        public OrderPackageDto OrderPackage { get; set; } = new();

        // Order Categories/Items
        public List<OrderCategoryAdminDto> OrderCategories { get; set; } = new();

        // Order Services
        public List<OrderServiceAdminDto> OrderServices { get; set; } = new();

        // Payment Information
        public List<OrderPaymentMethodAdminDto> OrderPaymentMethods { get; set; } = new();

        // Status History
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();

        // Additional Info
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}