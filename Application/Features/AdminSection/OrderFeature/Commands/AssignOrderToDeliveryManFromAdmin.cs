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

            public AssignOrderToDeliveryManFromAdminHandler(INaqlahContext context)
            {
                _context = context;
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

                var assignResult = order.AssignToDeliveryMan(request.DeliveryManId, DateTime.UtcNow);
                if (assignResult.IsFailure)
                    return Result.Failure<int>(assignResult.Error);

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}

