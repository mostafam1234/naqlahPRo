using Application.Features.CustomerSection.Feature.Notifications.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Notifications.Queries
{
    public sealed record GetCustomerNotificationsQuery : IRequest<Result<List<CustomerNotificationDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 20;

        private class Handler : IRequestHandler<GetCustomerNotificationsQuery, Result<List<CustomerNotificationDto>>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;

            public Handler(INaqlahContext context, IUserSession userSession)
            {
                _context = context;
                _userSession = userSession;
            }

            public async Task<Result<List<CustomerNotificationDto>>> Handle(GetCustomerNotificationsQuery request, CancellationToken cancellationToken)
            {
                var customerId = await _context.Customers
                    .Where(c => c.UserId == _userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                    return Result.Failure<List<CustomerNotificationDto>>("Customer not found.");

                var now = DateTime.UtcNow;

                var notifications = await _context.CustomerNotifications
                    .Include(cn => cn.Notification)
                    .Where(cn => cn.CustomerId == customerId
                        && (!cn.Notification.IsScheduled || cn.Notification.ScheduledDate <= now))
                    .OrderByDescending(cn => cn.CreatedDate)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(cn => new CustomerNotificationDto
                    {
                        Id = cn.Id,
                        NotificationId = cn.NotificationId,
                        ArabicTitle = cn.Notification.ArabicTitle,
                        EnglishTitle = cn.Notification.EnglishTitle,
                        ArabicMessage = cn.Notification.ArabicMessage,
                        EnglishMessage = cn.Notification.EnglishMessage,
                        OrderId = cn.Notification.OrderId,
                        NotificationType = cn.Notification.NotificationType,
                        IsRead = cn.IsRead,
                        ReadDate = cn.ReadDate,
                        CreatedDate = cn.CreatedDate
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(notifications);
            }
        }
    }
}
