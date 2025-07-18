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
    public sealed record AddDeliveryManDeviceTokenCommand:IRequest<Result>
    {
        public string AndriodDevice { get; set; } = string.Empty;
        public string IosDevice { get; set; } = string.Empty;

        private class AddDeliveryManDeviceTokenCommandHandler :
            IRequestHandler<AddDeliveryManDeviceTokenCommand, Result>
        {
            private readonly IUserSession userSession;
            private readonly INaqlahContext context;

            public AddDeliveryManDeviceTokenCommandHandler(IUserSession userSession,
                                                           INaqlahContext context)
            {
                this.userSession = userSession;
                this.context = context;
            }
            public async Task<Result> Handle(AddDeliveryManDeviceTokenCommand request, CancellationToken cancellationToken)
            {
                var deliveryMan = await context.DeliveryMen
                                              .AsTracking()
                                              .FirstOrDefaultAsync(x => x.UserId == userSession.UserId);

                if (deliveryMan is null)
                {
                    return Result.Failure("DeliveryMan Not Found");
                }

                var deviceResult = deliveryMan.SetFireBaseTokens(request.AndriodDevice,
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
