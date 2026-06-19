using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Order.Dtos
{
    public class AcceptedOrder
    {

        public int OrderId { get; set; }
        public bool IsScheduled { get;  set; }
        public DateTime? ExpectedPickUpTime { get;  set; }


        public string OrderNumber { get; set; } = string.Empty;


        public string CustomerPhone { get; set; } = string.Empty;


        public string CustomerName { get; set; } = string.Empty;


        public string CustomerNotes { get; set; } = string.Empty;


        public int OrderStatus { get; set; }


        public int OrderType { get; set; }


        public decimal Total { get; set; }


        public decimal DriverNetAmount { get; set; }


        public decimal TaxAmount { get; set; }


        public decimal ServiceFeeAmount { get; set; }


        public bool CanProceed { get; set; } = true;


        public bool DriverConfirmedGoingToPickup { get; set; }


        public bool ClientApprovedPickup { get; set; }


        public int CustomerId { get; set; }


        public List<WayPoint> WayPoints { get; set; } = new();


        public List<OrderDetail> OrderDetails { get; set; } = new();


        public List<OrderService> OrderServices { get; set; } = new();


        public string OrderPackageArabicDescription { get; set; } = string.Empty;


        public string OrderPackageEnglishDescription { get; set; } = string.Empty;


        public List<DeliveryPaymentMethodDto> PaymentMethods { get; set; } = new();
    }

    public class WayPoint
    {

        public int Id { get; set; }


        public double Latitude { get; set; }


        public double Longitude { get; set; }


        public int Status { get; set; }


        public string? PickedUpDate { get; set; }


        public string? PackImagePath { get; set; }


        public bool IsOrigin { get; set; }


        public bool IsDestination { get; set; }


        public bool BackAndForth { get; set; }


        public string RegionArabicName { get; set; } = string.Empty;


        public string RegionEnglishName { get; set; } = string.Empty;


        public string CityArabicName { get; set; } = string.Empty;


        public string CityEnglishName { get; set; } = string.Empty;


        public string NeighborhoodArabicName { get; set; } = string.Empty;


        public string NeighborhoodEnglishName { get; set; } = string.Empty;


    }

    public class OrderDetail
    {

        public int Id { get; set; }


        public int MainCategoryId { get; set; }

        public string ArabicCategoryName { get; set; } = string.Empty;


        public string EnglishCategoryName { get; set; } = string.Empty;
    }
    public class OrderService
    {
        
        public int Id { get; set; }

        
        public int WorkId { get; set; }

        
        public string ArabicName { get; set; } = string.Empty;

        
        public string EnglishName { get; set; } = string.Empty;

        
        public decimal Amount { get; set; }
    }

    public class DeliveryPaymentMethodDto
    {
        
        public int PaymentMethodId { get; set; }

       
        public string PaymentMethodArabicName { get; set; } = string.Empty;

       
        public string PaymentMethodEnglishName { get; set; } = string.Empty;

       
        public decimal Amount { get; set; }
    }

}