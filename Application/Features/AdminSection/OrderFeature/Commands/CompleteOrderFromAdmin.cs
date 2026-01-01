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
    public sealed record CompleteOrderFromAdmin : IRequest<Result<int>>
    {
        public int OrderId { get; init; }
        public int LanguageId { get; init; } = 1;

        private class CompleteOrderFromAdminHandler : IRequestHandler<CompleteOrderFromAdmin, Result<int>>
        {
            private readonly INaqlahContext _context;

            public CompleteOrderFromAdminHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(CompleteOrderFromAdmin request, CancellationToken cancellationToken)
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

                // Only assigned orders can be completed
                if (order.OrderStatus != OrderStatus.Assigned)
                {
                    var errMessage = request.LanguageId == 1 
                        ? "يمكن إكمال الطلبات المعينة فقط." 
                        : "Only assigned orders can be completed.";
                    return Result.Failure<int>(errMessage);
                }

                // Update order status to Completed
                var updateResult = order.UpdateStatus(OrderStatus.Completed, DateTime.UtcNow);
                if (updateResult.IsFailure)
                {
                    var errMessage = request.LanguageId == 1 
                        ? updateResult.Error 
                        : updateResult.Error;
                    return Result.Failure<int>(errMessage);
                }

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}

