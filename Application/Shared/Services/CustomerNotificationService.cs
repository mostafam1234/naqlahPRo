using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using IFcmService = Domain.InterFaces.INotificationService;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shared.Services
{
    public class CustomerNotificationService : ICustomerNotificationService
    {
        private readonly INaqlahContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IFcmService _fcmService;

        public CustomerNotificationService(INaqlahContext context, IDateTimeProvider dateTimeProvider, IFcmService fcmService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _fcmService = fcmService;
        }

        public async Task PrepareAsync(
            int customerId,
            int? orderId,
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            CancellationToken cancellationToken = default)
        {
            var notification = Notification.Create(
                arabicTitle, englishTitle,
                arabicMessage, englishMessage,
                notificationType,
                _dateTimeProvider.Now,
                orderId);

            notification.AddCustomerNotification(customerId, _dateTimeProvider.Now);
            _context.Notifications.Add(notification);

            var tokens = await _context.Customers
                .Where(c => c.Id == customerId)
                .Select(c => new { c.AndriodDevice, c.IosDevice })
                .FirstOrDefaultAsync(cancellationToken);

            if (tokens != null)
            {
                var firebaseTokens = new List<string> { tokens.AndriodDevice, tokens.IosDevice }
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (firebaseTokens.Any())
                {
                    await _fcmService.SendNotificationAsyncToMultipleDevices(new NotificationBodyForMultipleDevices
                    {
                        Title = arabicTitle,
                        Body = arabicMessage,
                        FireBaseTokens = firebaseTokens,
                        PayLoad = new Dictionary<string, string>
                        {
                            ["notificationType"] = ((int)notificationType).ToString(),
                            ["orderId"] = orderId?.ToString() ?? string.Empty
                        }
                    });
                }
            }
        }
    }
}
