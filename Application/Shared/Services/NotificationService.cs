using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using System.Threading.Tasks;

namespace Application.Shared.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INaqlahContext _context;
        readonly IDateTimeProvider dateTime;

        public NotificationService(INaqlahContext context, IDateTimeProvider dateTime)
        {
            this.dateTime = dateTime;
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            int? orderId = null,
            int? userId = null)
        {
            // Create notification in database
            var notification = Notification.Create(
                arabicTitle,
                englishTitle,
                arabicMessage,
                englishMessage,
                notificationType,
                dateTime.Now,
                orderId,
                userId);

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsyncWithResult();

            return notification;
        }
    }
}

