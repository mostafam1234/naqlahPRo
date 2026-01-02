using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Commands
{
    public class CancelOrderFromAdmin : IRequest<Result<int>>
    {
        public int OrderId { get; init; }
        public int LanguageId { get; init; } = 1; // Default to Arabic

        private class CancelOrderFromAdminHandler : IRequestHandler<CancelOrderFromAdmin, Result<int>>
        {
            private readonly INaqlahContext context;

            public CancelOrderFromAdminHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<int>> Handle(CancelOrderFromAdmin request, CancellationToken cancellationToken)
            {
                var order = await context.Orders
                    .AsTracking()
                    .Include(o => o.OrderStatusHistories)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order == null)
                {
                    var errMessage = request.LanguageId == 1 ? "الطلب غير موجود." : "Order not found.";
                    return Result.Failure<int>(errMessage);
                }

                if (order.OrderStatus == OrderStatus.Cancelled)
                {
                    var errMessage = request.LanguageId == 1 ? "الطلب ملغي بالفعل." : "Order is already canceled.";
                    return Result.Failure<int>(errMessage);
                }

                // Only pending orders can be canceled
                if (order.OrderStatus != OrderStatus.Pending)
                {
                    var errMessage = request.LanguageId == 1 ? "يمكن إلغاء الطلبات المعلقة فقط." : "Only Pending orders can be canceled.";
                    return Result.Failure<int>(errMessage);
                }

                order.CancelOrder(DateTime.UtcNow);

                await context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }

    }
}