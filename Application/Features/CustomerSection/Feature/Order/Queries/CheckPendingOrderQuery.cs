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
    public sealed record CheckPendingOrderQuery : IRequest<Result<int>>
    {
        private class CheckPendingOrderQueryHandler : IRequestHandler<CheckPendingOrderQuery, Result<int>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public CheckPendingOrderQueryHandler(INaqlahContext context, IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<int>> Handle(CheckPendingOrderQuery request, CancellationToken cancellationToken)
            {
                var customerId = await context.Customers
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                {
                    return Result.Failure<int>("Customer not found");
                }

                var pendingOrderId = await context.Orders
                    .OrderByDescending(o => o.Id)
                    .Where(o => o.CustomerId == customerId && o.OrderStatus == OrderStatus.Pending)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                return Result.Success(pendingOrderId);
            }


        }
    }
}