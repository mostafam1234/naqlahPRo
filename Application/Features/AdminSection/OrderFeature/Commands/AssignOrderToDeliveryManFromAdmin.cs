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
                {
                    var errMessage = request.LanguageId == 1 ? "الطلب غير موجود." : "Order not found.";
                    return Result.Failure<int>(errMessage);
                }

                // Only pending orders can be assigned
                if (order.OrderStatus != OrderStatus.Pending)
                {
                    var errMessage = request.LanguageId == 1 
                        ? "يمكن تعيين الطلبات المعلقة فقط." 
                        : "Only pending orders can be assigned.";
                    return Result.Failure<int>(errMessage);
                }

                // Verify delivery man exists and is available
                var deliveryMan = await _context.DeliveryMen
                    .FirstOrDefaultAsync(dm => dm.Id == request.DeliveryManId 
                                            && dm.DeliveryState == DeliveryRequesState.Approved 
                                            && dm.Active, cancellationToken);

                if (deliveryMan == null)
                {
                    var errMessage = request.LanguageId == 1 
                        ? "مندوب التوصيل غير موجود أو غير متاح." 
                        : "Delivery man not found or not available.";
                    return Result.Failure<int>(errMessage);
                }

                // Check if delivery man already has an active order
                var hasActiveOrder = await _context.Orders
                    .AnyAsync(o => o.DeliveryManId == request.DeliveryManId 
                                && o.OrderStatus == OrderStatus.Assigned 
                                && o.Id != request.OrderId, cancellationToken);

                if (hasActiveOrder)
                {
                    var errMessage = request.LanguageId == 1 
                        ? "مندوب التوصيل لديه طلب نشط بالفعل." 
                        : "Delivery man already has an active order.";
                    return Result.Failure<int>(errMessage);
                }

                // Assign the delivery man
                var assignResult = order.AssignToDeliveryMan(request.DeliveryManId, DateTime.UtcNow);
                if (assignResult.IsFailure)
                {
                    var errMessage = request.LanguageId == 1 
                        ? assignResult.Error 
                        : assignResult.Error;
                    return Result.Failure<int>(errMessage);
                }

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}

