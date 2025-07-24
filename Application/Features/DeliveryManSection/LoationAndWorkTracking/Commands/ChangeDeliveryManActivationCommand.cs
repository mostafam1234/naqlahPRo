using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.LoationAndWorkTracking.Commands
{
    public sealed record ChangeDeliveryManActivationCommand:IRequest<Result>
    {
        public bool Active { get; set; }

        private class ChangeDeliveryManActivationCommandHandler :
            IRequestHandler<ChangeDeliveryManActivationCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public ChangeDeliveryManActivationCommandHandler(INaqlahContext context,
                                                             IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(ChangeDeliveryManActivationCommand request, CancellationToken cancellationToken)
            {
                var deliveryMan = await context.DeliveryMen
                                             .AsTracking()
                                             .FirstOrDefaultAsync(x => x.UserId == userSession.UserId);

                if(deliveryMan is null)
                {
                    return Result.Failure("Delivery Man Not Found");
                }

                deliveryMan.ChangeActivation(request.Active);
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
