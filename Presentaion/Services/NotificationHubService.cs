using Application.Shared.Services;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Presentaion.Hubs;
using System.Linq;
using System.Threading.Tasks;

namespace Presentaion.Services
{
    public class NotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext, 
            INotificationService notificationService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task SendNotificationAsync(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            int? orderId = null,
            int? userId = null)
        {
            // Create notification in database
            var notification = await _notificationService.CreateNotificationAsync(
                arabicTitle,
                englishTitle,
                arabicMessage,
                englishMessage,
                notificationType,
                orderId,
                userId);

            // Send notification via SignalR
            var notificationDto = new
            {
                Id = notification.Id,
                ArabicTitle = notification.ArabicTitle,
                EnglishTitle = notification.EnglishTitle,
                ArabicMessage = notification.ArabicMessage,
                EnglishMessage = notification.EnglishMessage,
                OrderId = notification.OrderId,
                NotificationType = notification.NotificationType,
                CreationDate = notification.CreationDate,
                IsRead = notification.IsRead
            };

            // Send to all connected clients (or specific user if userId is provided)
            try
            {
                if (userId.HasValue)
                {
                    System.Console.WriteLine($"[NotificationHubService] Sending notification to user {userId.Value}");
                    await _hubContext.Clients.User(userId.Value.ToString()).SendAsync("NewNotification", notificationDto);
                    System.Console.WriteLine($"[NotificationHubService] Notification sent to user {userId.Value}");
                }
                else
                {
                    System.Console.WriteLine("[NotificationHubService] Sending notification to all connected clients");
                    await _hubContext.Clients.All.SendAsync("NewNotification", notificationDto);
                    System.Console.WriteLine("[NotificationHubService] Notification sent to all clients");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[NotificationHubService] Error sending notification: {ex.Message}");
                System.Console.WriteLine($"[NotificationHubService] Stack trace: {ex.StackTrace}");
            }
        }
    }
}

