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
    public sealed record SaveDeliveryManLocationCommand:IRequest<Result>
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        private class SaveDeliveryManLocationCommandHandler :
            IRequestHandler<SaveDeliveryManLocationCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public SaveDeliveryManLocationCommandHandler(INaqlahContext context,
                                                         IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveDeliveryManLocationCommand request, CancellationToken cancellationToken)
            {
                var deliveryMan = await context.DeliveryMen
                                                .Include(x=>x.DeliveryManLocation)
                                               .AsTracking()
                                               .FirstOrDefaultAsync(x => x.UserId == userSession.UserId);

                if (deliveryMan is null)
                {
                    return Result.Failure("Delivery Man Not Found");
                }

                deliveryMan.SaveLocation(request.Longitude, request.Latitude);
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }


    }
}
