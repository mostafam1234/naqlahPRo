using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Commands
{
    public sealed record CancelCustomerOrderCommand : IRequest<Result>
    {
        public int OrderId { get; set; }


        public class CancelCustomerOrderCommandHandler : IRequestHandler<CancelCustomerOrderCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IDateTimeProvider dateTimeProvider;

            public CancelCustomerOrderCommandHandler(INaqlahContext context,IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.dateTimeProvider = dateTimeProvider;
            }
            public async Task<Result> Handle(CancelCustomerOrderCommand request, CancellationToken cancellationToken)
            {
                var order = await context.Orders.AsTracking().Include(x=>x.OrderStatusHistories).FirstOrDefaultAsync(x=>x.Id==request.OrderId, cancellationToken);
                if (order == null)
                {
                    return Result.Failure("Order not found");
                }

                order.CancelOrder(dateTimeProvider.Now);
                var saveResult=await context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure(saveResult.Error);
                }

                return Result.Success();
            }
        }
    }
}
