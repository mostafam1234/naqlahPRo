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
    public sealed record CompleteOrderFromAdmin : IRequest<Result<int>>
    {
        public int OrderId { get; init; }
        public int LanguageId { get; init; } = 1;

        private class CompleteOrderFromAdminHandler : IRequestHandler<CompleteOrderFromAdmin, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly ICustomerNotificationService _customerNotificationService;
            private readonly IDateTimeProvider _dateTimeProvider;

            public CompleteOrderFromAdminHandler(INaqlahContext context,
                ICustomerNotificationService customerNotificationService,
                IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                _customerNotificationService = customerNotificationService;
                _dateTimeProvider = dateTimeProvider;
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

                if (order.OrderStatus != OrderStatus.Assigned)
                {
                    var errMessage = request.LanguageId == 1
                        ? "يمكن إكمال الطلبات المعينة فقط."
                        : "Only assigned orders can be completed.";
                    return Result.Failure<int>(errMessage);
                }

                var updateResult = order.UpdateStatus(OrderStatus.Completed, _dateTimeProvider.Now);
                if (updateResult.IsFailure)
                {
                    return Result.Failure<int>(updateResult.Error);
                }

                await _customerNotificationService.PrepareAsync(
                    order.CustomerId,
                    order.Id,
                    "تم إكمال الطلب",
                    "Order Completed",
                    $"تم إكمال طلبك رقم {order.OrderNumber} بنجاح، شكراً لاستخدامك نقلة",
                    $"Your order #{order.OrderNumber} has been completed. Thank you for using Naqlah!",
                    NotificationType.OrderCompleted,
                    cancellationToken);

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}
