using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.LogIn.Commands
{
    public sealed record RemoveDeliveryManDeviceTokenCommand : IRequest<Result>
    {
        public string AndriodDevice { get; set; } = string.Empty;
        public string IosDevice { get; set; } = string.Empty;

        private class RemoveDeliveryManDeviceTokenCommandHandler :
            IRequestHandler<RemoveDeliveryManDeviceTokenCommand, Result>
        {
            private readonly IUserSession userSession;
            private readonly INaqlahContext context;

            public RemoveDeliveryManDeviceTokenCommandHandler(IUserSession userSession,
                                                              INaqlahContext context)
            {
                this.userSession = userSession;
                this.context = context;
            }
            public async Task<Result> Handle(RemoveDeliveryManDeviceTokenCommand request, CancellationToken cancellationToken)
            {
                var deliveryMan = await context.DeliveryMen
                                               .AsTracking()
                                               .FirstOrDefaultAsync(x => x.UserId == userSession.UserId);

                if (deliveryMan is null)
                {
                    return Result.Failure("DeliveryMan Not Found");
                }

                var deviceResult = deliveryMan.RemoveFireBaseTokens(request.AndriodDevice,
                                                                    request.IosDevice);

                if (deviceResult.IsFailure)
                {
                    return deviceResult;
                }

                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
