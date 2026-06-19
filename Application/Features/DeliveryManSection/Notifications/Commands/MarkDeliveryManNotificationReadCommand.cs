using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Notifications.Commands
{
    public sealed record MarkDeliveryManNotificationReadCommand : IRequest<Result>
    {
        public int NotificationId { get; init; }

        private class Handler : IRequestHandler<MarkDeliveryManNotificationReadCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IDateTimeProvider dateTimeProvider;

            public Handler(INaqlahContext context, IUserSession userSession, IDateTimeProvider dateTimeProvider)
            {
                this.context = context;
                this.userSession = userSession;
                this.dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result> Handle(MarkDeliveryManNotificationReadCommand request, CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                    .Where(x => x.UserId == userSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryManId == 0)
                {
                    return Result.Failure("Delivery man not found");
                }

                var notification = await context.CaptainNotifications
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.NotificationId && x.DeliveryManId == deliveryManId, cancellationToken);

                if (notification is null)
                {
                    return Result.Failure("Notification not found");
                }

                notification.MarkAsRead(dateTimeProvider.Now);

                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
