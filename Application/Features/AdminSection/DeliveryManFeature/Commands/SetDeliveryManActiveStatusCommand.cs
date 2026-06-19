using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Commands
{
    public sealed record SetDeliveryManActiveStatusCommand : IRequest<Result<SetDeliveryManActiveStatusResultDto>>
    {
        public int DeliveryManId { get; init; }
        public bool Active { get; init; }

        private sealed class Handler : IRequestHandler<SetDeliveryManActiveStatusCommand, Result<SetDeliveryManActiveStatusResultDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;

            public Handler(INaqlahContext context, IUserSession userSession)
            {
                _context = context;
                _userSession = userSession;
            }

            public async Task<Result<SetDeliveryManActiveStatusResultDto>> Handle(
                SetDeliveryManActiveStatusCommand request,
                CancellationToken cancellationToken)
            {
                var deliveryMan = await _context.DeliveryMen
                    .AsTracking()
                    .FirstOrDefaultAsync(dm => dm.Id == request.DeliveryManId, cancellationToken);

                if (deliveryMan is null)
                    return Result.Failure<SetDeliveryManActiveStatusResultDto>("DeliveryManNotFound");

                if (deliveryMan.DeliveryState != DeliveryRequesState.Approved)
                    return Result.Failure<SetDeliveryManActiveStatusResultDto>("OnlyApprovedCaptainsCanChangeActiveStatus");

                var statusChanged = DeliveryManActiveHistoryAppender.ApplyIfChanged(
                    _context,
                    deliveryMan,
                    request.Active,
                    _userSession.UserId);

                if (statusChanged)
                {
                    var saveResult = await _context.SaveChangesAsyncWithResult();
                    if (saveResult.IsFailure)
                        return Result.Failure<SetDeliveryManActiveStatusResultDto>(saveResult.Error);
                }

                return Result.Success(new SetDeliveryManActiveStatusResultDto
                {
                    DeliveryManId = deliveryMan.Id,
                    Active = deliveryMan.Active,
                    ActiveStatusName = DeliveryManDisplayLabels.GetActiveStatusName(deliveryMan.Active, _userSession.LanguageId),
                    StatusChanged = statusChanged
                });
            }
        }
    }
}
