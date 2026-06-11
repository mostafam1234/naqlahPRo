using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Jobs
{
    public class ProcessScheduledNotificationsJob
    {
        private readonly INaqlahContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INotificationService _fcmService;

        public ProcessScheduledNotificationsJob(INaqlahContext context, IDateTimeProvider dateTimeProvider, INotificationService fcmService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _fcmService = fcmService;
        }

        public async Task ProcessAsync()
        {
            var now = _dateTimeProvider.Now;

            var dueNotifications = await _context.Notifications
                .AsTracking()
                .Where(n => n.IsAdminBroadcast && n.IsScheduled && !n.IsProcessed && n.ScheduledDate <= now)
                .ToListAsync();

            if (!dueNotifications.Any())
                return;

            var pushQueue = new List<(string title, string body, NotificationType type, List<string> tokens)>();

            foreach (var notification in dueNotifications)
            {
                var customerQuery = _context.Customers.AsQueryable();

                if (notification.TargetCustomerType.HasValue)
                    customerQuery = customerQuery.Where(c => (int)c.CustomerType == notification.TargetCustomerType.Value);

                var customers = await customerQuery
                    .Select(c => new { c.Id, c.AndriodDevice, c.IosDevice })
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    var cn = CustomerNotification.InstanceWithNotificationId(notification.Id, customer.Id, now);
                    await _context.CustomerNotifications.AddAsync(cn);
                }

                notification.MarkAsProcessed();

                var tokens = customers
                    .SelectMany(c => new[] { c.AndriodDevice, c.IosDevice })
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (tokens.Any())
                    pushQueue.Add((notification.ArabicTitle, notification.ArabicMessage, notification.NotificationType, tokens));
            }

            await _context.SaveChangesAsyncWithResult();

            foreach (var (title, body, type, tokens) in pushQueue)
            {
                for (int i = 0; i < tokens.Count; i += 500)
                {
                    var batch = tokens.Skip(i).Take(500).ToList();
                    await _fcmService.SendNotificationAsyncToMultipleDevices(new NotificationBodyForMultipleDevices
                    {
                        Title = title,
                        Body = body,
                        FireBaseTokens = batch,
                        PayLoad = new Dictionary<string, string>
                        {
                            ["notificationType"] = ((int)type).ToString()
                        }
                    });
                }
            }
        }
    }
}
