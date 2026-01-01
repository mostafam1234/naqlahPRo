using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Commands
{
    public sealed record MarkNotificationAsReadCommand : IRequest<Result<int>>
    {
        public int NotificationId { get; init; }
        public int LanguageId { get; init; } = 1;

        private class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<int>>
        {
            private readonly INaqlahContext _context;

            public MarkNotificationAsReadCommandHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<int>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

                if (notification == null)
                {
                    var errorMessage = request.LanguageId == 1 ? "الإشعار غير موجود." : "Notification not found.";
                    return Result.Failure<int>(errorMessage);
                }

                notification.MarkAsRead();

                await _context.SaveChangesAsyncWithResult();

                return Result.Success(notification.Id);
            }
        }
    }
}

