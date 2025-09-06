using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Order.Queries
{
    public sealed record GetOrderDetailsByIdQuery(int OrderId) : IRequest<Result<OrderDetailsResponse>>;

    public sealed record OrderDetailsResponse
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus OrderStatus { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        public List<OrderWayPointResponse> WayPoints { get; set; } = new();
        public List<OrderDetailItemResponse> OrderDetails { get; set; } = new();
        public List<OrderServiceResponse> OrderServices { get; set; } = new();
        public string OrderPackageArabicDescription { get; set; } = string.Empty;
        public string OrderPackageEnglishDescription { get; set; } = string.Empty;
        public List<PaymentMethodResponse> PaymentMethods { get; set; } = new();
    }

    public sealed record OrderWayPointResponse
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public OrderWayPointsStatus Status { get; set; }
        public DateTime? PickedUpDate { get; set; }
        public string PackImagePath { get; set; } = string.Empty;
        public bool IsOrigin { get; set; }
        public bool IsDestination { get; set; }
        public string RegionArabicName { get; set; } = string.Empty;
        public string RegionEnglishName { get; set; } = string.Empty;
        public string CityArabicName { get; set; } = string.Empty;
        public string CityEnglishName { get; set; } = string.Empty;
        public string NeighborhoodArabicName { get; set; } = string.Empty;
        public string NeighborhoodEnglishName { get; set; } = string.Empty;
    }

    public sealed record OrderDetailItemResponse
    {
        public int Id { get; set; }
        public int MainCategoryId { get; set; }
        public string ArabicCategoryName { get; set; } = string.Empty;
        public string EnglishCategoryName { get; set; } = string.Empty;
    }

    public sealed record OrderServiceResponse
    {
        public int Id { get; set; }
        public int WorkId { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public sealed record PaymentMethodResponse
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodArabicName { get; set; } = string.Empty;
        public string PaymentMethodEnglishName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    internal class GetOrderDetailsByIdQueryHandler : IRequestHandler<GetOrderDetailsByIdQuery, Result<OrderDetailsResponse>>
    {
        private readonly INaqlahContext context;
        private readonly IUserSession userSession;

        public GetOrderDetailsByIdQueryHandler(INaqlahContext context, IUserSession userSession)
        {
            this.context = context;
            this.userSession = userSession;
        }

        public async Task<Result<OrderDetailsResponse>> Handle(GetOrderDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = userSession.UserId;
            
            var deliveryMan = await context.DeliveryMen
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (deliveryMan is null)
            {
                return Result.Failure<OrderDetailsResponse>("Delivery man not found");
            }

            var order = await context.Orders
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.Region)
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.City)
                .Include(o => o.OrderWayPoints)
                    .ThenInclude(wp => wp.Neighborhood)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MainCategory)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.AssistanWork)
                .Include(o => o.PaymentMethods)
                    .ThenInclude(pm => pm.PaymentMethod)
                .Include(o => o.OrderPackage)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.DeliveryManId == deliveryMan.Id, cancellationToken);

            if (order is null)
            {
                return Result.Failure<OrderDetailsResponse>("Order not found or not assigned to you");
            }

            var response = new OrderDetailsResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                OrderType = order.OrderType,
                Total = order.Total,
                CustomerId = order.CustomerId,
                OrderPackageArabicDescription = order.OrderPackage?.ArabicDescripton ?? string.Empty,
                OrderPackageEnglishDescription = order.OrderPackage?.EnglishDescription ?? string.Empty,
                WayPoints = order.OrderWayPoints.Select(wp => new OrderWayPointResponse
                {
                    Id = wp.Id,
                    Latitude = wp.Latitude,
                    Longitude = wp.longitude,
                    Status = wp.OrderWayPointsStatus,
                    PickedUpDate = wp.PickedUpDate,
                    PackImagePath = wp.PackImagePath,
                    IsOrigin = wp.IsOrgin,
                    IsDestination = wp.IsDestination,
                    RegionArabicName = wp.Region?.ArabicName ?? string.Empty,
                    RegionEnglishName = wp.Region?.EnglishName ?? string.Empty,
                    CityArabicName = wp.City?.ArabicName ?? string.Empty,
                    CityEnglishName = wp.City?.EnglishName ?? string.Empty,
                    NeighborhoodArabicName = wp.Neighborhood?.ArabicName ?? string.Empty,
                    NeighborhoodEnglishName = wp.Neighborhood?.EnglishName ?? string.Empty
                }).OrderBy(wp => wp.IsOrigin ? 0 : (wp.IsDestination ? 2 : 1)).ToList(),
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailItemResponse
                {
                    Id = od.Id,
                    MainCategoryId = od.MainCategoryId,
                    ArabicCategoryName = od.ArabicCategoryName,
                    EnglishCategoryName = od.EnglishCategoryName
                }).ToList(),
                OrderServices = order.OrderServices.Select(os => new OrderServiceResponse
                {
                    Id = os.Id,
                    WorkId = os.WorkId,
                    ArabicName = os.ArabicName,
                    EnglishName = os.EnglishName,
                    Amount = os.Amount
                }).ToList(),
                PaymentMethods = order.PaymentMethods.Select(pm => new PaymentMethodResponse
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    PaymentMethodArabicName = pm.PaymentMethod?.ArabicName ?? string.Empty,
                    PaymentMethodEnglishName = pm.PaymentMethod?.EnglishName ?? string.Empty,
                    Amount = pm.Amount
                }).ToList()
            };

            return Result.Success(response);
        }
    }
}