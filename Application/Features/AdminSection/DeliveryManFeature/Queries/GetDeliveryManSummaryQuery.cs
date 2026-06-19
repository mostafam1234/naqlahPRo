using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetDeliveryManSummaryQuery : IRequest<Result<DeliveryManSummaryDto>>
    {
        public int DeliveryManId { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetDeliveryManSummaryQuery, Result<DeliveryManSummaryDto>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<DeliveryManSummaryDto>> Handle(
                GetDeliveryManSummaryQuery request,
                CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                var captain = await (
                    from dm in _context.DeliveryMen
                    join user in _context.Users on dm.UserId equals user.Id
                    join vehicle in _context.DeliveryVehicles on dm.Id equals vehicle.DeliveryManId into vehicleGroup
                    from vehicle in vehicleGroup.DefaultIfEmpty()
                    join vehicleType in _context.VehicleTypes on vehicle.VehicleTypeId equals vehicleType.Id into vehicleTypeGroup
                    from vehicleType in vehicleTypeGroup.DefaultIfEmpty()
                    where dm.Id == request.DeliveryManId && dm.DeliveryState == DeliveryRequesState.Approved
                    select new
                    {
                        dm.Id,
                        dm.FullName,
                        dm.PhoneNumber,
                        dm.Active,
                        dm.HasIncompleteRegistration,
                        UserEmail = user.Email ?? string.Empty,
                        VehicleTypeName = vehicleType != null
                            ? (isArabic ? vehicleType.ArabicName : vehicleType.EnglishName)
                            : string.Empty,
                        VehiclePlate = vehicle != null ? vehicle.LicensePlateNumber : string.Empty
                    }).FirstOrDefaultAsync(cancellationToken);

                if (captain == null)
                    return Result.Failure<DeliveryManSummaryDto>("DeliveryManNotFound");

                var statusCounts = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.DeliveryManId == request.DeliveryManId)
                    .GroupBy(o => o.OrderStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var countLookup = statusCounts.ToDictionary(x => x.Status, x => x.Count);

                int Count(OrderStatus status) => countLookup.TryGetValue(status, out var count) ? count : 0;

                var pending = Count(OrderStatus.Pending);
                var assigned = Count(OrderStatus.Assigned);
                var confirmed = Count(OrderStatus.ConfirmedGoingToPickup);
                var pickedUp = Count(OrderStatus.PickedUpFromDeliveryMan);
                var completed = Count(OrderStatus.Completed);
                var cancelled = Count(OrderStatus.Cancelled);

                var ordersByStatus = new List<DeliveryManOrderStatusCountDto>
                {
                    BuildStatusCount(OrderStatus.Pending, pending, request.LanguageId),
                    BuildStatusCount(OrderStatus.Assigned, assigned, request.LanguageId),
                    BuildStatusCount(OrderStatus.ConfirmedGoingToPickup, confirmed, request.LanguageId),
                    BuildStatusCount(OrderStatus.PickedUpFromDeliveryMan, pickedUp, request.LanguageId),
                    BuildStatusCount(OrderStatus.Completed, completed, request.LanguageId),
                    BuildStatusCount(OrderStatus.Cancelled, cancelled, request.LanguageId)
                };

                var summary = new DeliveryManSummaryDto
                {
                    Id = captain.Id,
                    FullName = captain.FullName,
                    PhoneNumber = captain.PhoneNumber,
                    Email = captain.UserEmail,
                    Active = captain.Active,
                    VehicleTypeName = captain.VehicleTypeName,
                    VehiclePlate = captain.VehiclePlate,
                    HasIncompleteRegistration = captain.HasIncompleteRegistration,
                    ProfileCompletenessLabel = captain.HasIncompleteRegistration
                        ? (isArabic ? "بيانات غير مكتملة" : "Incomplete profile")
                        : (isArabic ? "بيانات مكتملة" : "Complete profile"),
                    TotalOrders = statusCounts.Sum(x => x.Count),
                    PendingOrders = pending,
                    AssignedOrders = assigned,
                    ConfirmedGoingToPickupOrders = confirmed,
                    PickedUpOrders = pickedUp,
                    CompletedOrders = completed,
                    CancelledOrders = cancelled,
                    ActiveOrders = assigned + confirmed + pickedUp,
                    OrdersByStatus = ordersByStatus
                };

                return Result.Success(summary);
            }

            private static DeliveryManOrderStatusCountDto BuildStatusCount(
                OrderStatus status,
                int count,
                int languageId)
            {
                return new DeliveryManOrderStatusCountDto
                {
                    Status = status,
                    StatusName = OrderDisplayLabels.GetOrderStatusName(status, languageId),
                    Count = count
                };
            }
        }
    }
}
