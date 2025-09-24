using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.NewRequests.Commands
{
    public sealed record UpdateDeliveryManState: IRequest<Result<string>>
    {
        public int DeliveryId { get; set; }
        public int State { get; set; }
        private class UpdateDeliveryManStateHandler : IRequestHandler<UpdateDeliveryManState, Result<string>>
        {
            private readonly INaqlahContext _context;
            public UpdateDeliveryManStateHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<string>> Handle(UpdateDeliveryManState request, CancellationToken cancellationToken)
            {
                var deliveryMan = await _context.DeliveryMen.AsTracking().Where(x => x.Id == request.DeliveryId).FirstOrDefaultAsync();
                if(deliveryMan == null)
                {
                    return Result.Failure<string>("Delivery Man Not Exists");
                }
                
                deliveryMan.UpdateDeliveryManRequestState(request.State);
                await _context.SaveChangesAsyncWithResult();

                return Result.Success("Saved");
            }
        }
    }
}
