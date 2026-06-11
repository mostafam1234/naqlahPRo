using Application.Features.AdminSection.NotificationFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Queries
{
    public sealed record GetBroadcastNotificationsQuery : IRequest<Result<PagedResult<BroadcastNotificationDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 20;
        public bool? ScheduledOnly { get; init; }

        private class Handler : IRequestHandler<GetBroadcastNotificationsQuery, Result<PagedResult<BroadcastNotificationDto>>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<BroadcastNotificationDto>>> Handle(GetBroadcastNotificationsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Notifications
                    .Where(n => n.IsAdminBroadcast);

                if (request.ScheduledOnly == true)
                    query = query.Where(n => n.IsScheduled && !n.IsProcessed);

                var totalCount = await query.CountAsync(cancellationToken);

                var data = await query
                    .OrderByDescending(n => n.CreationDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(n => new BroadcastNotificationDto
                    {
                        Id = n.Id,
                        ArabicTitle = n.ArabicTitle,
                        EnglishTitle = n.EnglishTitle,
                        ArabicMessage = n.ArabicMessage,
                        EnglishMessage = n.EnglishMessage,
                        NotificationType = n.NotificationType,
                        TargetAll = n.TargetAll,
                        TargetCustomerType = n.TargetCustomerType,
                        IsScheduled = n.IsScheduled,
                        ScheduledDate = n.ScheduledDate,
                        IsProcessed = n.IsProcessed,
                        RecipientCount = n.CustomerNotifications.Count(),
                        CreationDate = n.CreationDate
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(new PagedResult<BroadcastNotificationDto>
                {
                    Data = data,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.Take)
                });
            }
        }
    }
}
