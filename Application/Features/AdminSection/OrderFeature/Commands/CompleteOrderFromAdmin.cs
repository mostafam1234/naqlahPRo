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
                    return Result.Failure<int>("OrderNotFound");

                if (order.OrderStatus != OrderStatus.Assigned)
                    return Result.Failure<int>("OnlyAssignedOrdersCanBeCompleted");

                var updateResult = order.UpdateStatus(OrderStatus.Completed, DateTime.UtcNow);
                if (updateResult.IsFailure)
                    return Result.Failure<int>(updateResult.Error);

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(order.Id);
            }
        }
    }
}

