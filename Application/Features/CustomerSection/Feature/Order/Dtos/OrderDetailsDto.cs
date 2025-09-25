using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public decimal Total { get; set; }
        public OrderType OrderType { get; set; }
        public string OrderTypeName { get; set; }
        
        // Vehicle Information
        public int? VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }
        
        // Delivery Man Information
        public int? DeliveryManId { get; set; }
        public string DeliveryManName { get; set; }
        public string DeliveryManPhone { get; set; }
        
        // Package Information
        public string PackageName { get; set; }
        
        // Categories
        public List<OrderCategoryDto> Categories { get; set; }
        
        // Services
        public List<OrderServiceDto> Services { get; set; }
        
        // Waypoints
        public List<OrderWayPointDto> WayPoints { get; set; }
        
        // Payment Methods
        public List<OrderPaymentMethodDto> PaymentMethods { get; set; }

        public OrderDetailsDto()
        {
            Categories = new List<OrderCategoryDto>();
            Services = new List<OrderServiceDto>();
            WayPoints = new List<OrderWayPointDto>();
            PaymentMethods = new List<OrderPaymentMethodDto>();
        }
    }

    public class OrderCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class OrderServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Amount { get; set; }
    }

    public class OrderWayPointDto
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsOrigin { get; set; }
        public bool IsDestination { get; set; }
        public OrderWayPointsStatus Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? PickedUpDate { get; set; }
        public string Address { get; set; }
        public string RegionName { get; set; }
        public string CityName { get; set; }
        public string NeighborhoodName { get; set; }
    }

    public class OrderPaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
    }
}