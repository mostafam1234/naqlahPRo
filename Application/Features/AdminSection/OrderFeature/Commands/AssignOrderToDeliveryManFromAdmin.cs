using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public sealed record AssignOrderToDeliveryManFromAdmin : IRequest<Result<int>>
    {
        public int OrderId { get; init; }
        public int DeliveryManId { get; init; }
        public int LanguageId { get; init; } = 1;

        private class AssignOrderToDeliveryManFromAdminHandler : IRequestHandler<AssignOrderToDeliveryManFromAdmin, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly ICustomerNotificationService _customerNotificationService;
            private readonly IDateTimeProvider _dateTimeProvider;

            public AssignOrderToDeliveryManFromAdminHandler(INaqlahContext context,
                ICustomerNotificationService customerNotificationService,
                IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                _customerNotificationService = customerNotificationService;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result<int>> Handle(AssignOrderToDeliveryManFromAdmin request, CancellationToken cancellationToken)
            {
                var order = await _context.Orders
                    .AsTracking()
                    .Include(o => o.OrderStatusHistories)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order == null)
                    return Result.Failure<int>("OrderNotFound");

                if (order.OrderStatus != OrderStatus.Pending)
                    return Result.Failure<int>("OnlyPendingOrdersCanBeAssigned");

                // Verify delivery man exists and is available
                var deliveryMan = await _context.DeliveryMen
                    .FirstOrDefaultAsync(dm => dm.Id == request.DeliveryManId 
                                            && dm.DeliveryState == DeliveryRequesState.Approved 
                                            && dm.Active, cancellationToken);

                if (deliveryMan == null)
                    return Result.Failure<int>("DeliveryManNotFoundOrNotAvailable");

                // Check if delivery man already has an active order
                var hasActiveOrder = await _context.Orders
                    .AnyAsync(o => o.DeliveryManId == request.DeliveryManId 
                                && o.OrderStatus == OrderStatus.Assigned 
                                && o.Id != request.OrderId, cancellationToken);

                if (hasActiveOrder)
                    return Result.Failure<int>("DeliveryManAlreadyHasActiveOrder");

                // Assign the delivery man
                var assignResult = order.AssignToDeliveryMan(request.DeliveryManId, _dateTimeProvider.Now);
                if (assignResult.IsFailure)
                    return Result.Failure<int>(assignResult.Error);

                await _customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تم تعيين الكابتن",
                    "Captain Assigned",
                    $"تم تعيين كابتن لطلبك رقم {order.OrderNumber}",
                    $"A captain has been assigned to your order #{order.OrderNumber}",
                    NotificationType.CaptainAssigned,
                    cancellationToken);

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}

