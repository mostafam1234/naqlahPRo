using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Commands
{
    public sealed record MarkAllNotificationsAsReadCommand : IRequest<Result<int>>
    {
        public int LanguageId { get; init; } = 1;

        private class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public MarkAllNotificationsAsReadCommandHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => !n.IsRead)
                    .ToListAsync(cancellationToken);

                foreach (var notification in unreadNotifications)
                {
                    notification.MarkAsRead();
                }

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(unreadNotifications.Count);
            }
        }
    }
}

