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

namespace Application.Features.CustomerSection.Feature.Order.Queries
{
    public sealed record CheckPendingOrderQuery : IRequest<Result<bool>>
    {
        private class CheckPendingOrderQueryHandler : IRequestHandler<CheckPendingOrderQuery, Result<bool>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public CheckPendingOrderQueryHandler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<bool>> Handle(CheckPendingOrderQuery request, CancellationToken cancellationToken)
            {
                var customerId = await context.Customers
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<bool>("Customer not found");
                }

                var hasPendingOrder = await context.Orders
                    .AnyAsync(o => o.CustomerId == customerId && o.OrderStatus == OrderStatus.Pending, cancellationToken);

                return Result.Success(hasPendingOrder);
            }
        }
    }
}