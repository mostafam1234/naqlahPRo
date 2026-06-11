using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Notifications.Commands
{
    public sealed record MarkNotificationReadCommand : IRequest<Result>
    {
        public int CustomerNotificationId { get; init; }

        private class Handler : IRequestHandler<MarkNotificationReadCommand, Result>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;
            private readonly IDateTimeProvider _dateTimeProvider;

            public Handler(INaqlahContext context, IUserSession userSession, IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                _userSession = userSession;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
            {
                var customerId = await _context.Customers
                    .Where(c => c.UserId == _userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                    return Result.Failure("Customer not found.");

                var cn = await _context.CustomerNotifications
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.CustomerNotificationId && x.CustomerId == customerId, cancellationToken);

                if (cn == null)
                    return Result.Failure("Notification not found.");

                if (cn.IsRead)
                    return Result.Success();

                cn.MarkAsRead(_dateTimeProvider.Now);

                return await _context.SaveChangesAsyncWithResult();
            }
        }
    }
}
