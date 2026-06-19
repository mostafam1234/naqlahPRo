using Application.Shared.Services;
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

namespace Application.Features.DeliveryManSection.NewRequests.Commands
{
    public sealed record UpdateDeliveryManState: IRequest<Result<string>>
    {
        public int DeliveryId { get; set; }
        public int State { get; set; }
        public int? ChangedByUserId { get; set; }

        private class UpdateDeliveryManStateHandler : IRequestHandler<UpdateDeliveryManState, Result<string>>
        {
            private readonly INaqlahContext _context;
            public UpdateDeliveryManStateHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<string>> Handle(UpdateDeliveryManState request, CancellationToken cancellationToken)
            {
                var deliveryMan = await _context.DeliveryMen.AsTracking().Where(x => x.Id == request.DeliveryId).FirstOrDefaultAsync(cancellationToken);
                if(deliveryMan == null)
                {
                    return Result.Failure<string>("DeliveryManNotExists");
                }
                
                deliveryMan.UpdateDeliveryManRequestState(request.State);
                
                if (request.State == (int)DeliveryRequesState.Approved)
                {
                    DeliveryManActiveHistoryAppender.ApplyIfChanged(
                        _context,
                        deliveryMan,
                        true,
                        request.ChangedByUserId);
                }
                
                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure<string>(saveResult.Error);
                }

                return Result.Success("Saved");
            }
        }
    }
}
