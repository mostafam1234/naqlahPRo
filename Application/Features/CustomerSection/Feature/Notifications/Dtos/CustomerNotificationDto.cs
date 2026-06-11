using Domain.Enums;
using System;

namespace Application.Features.CustomerSection.Feature.Notifications.Dtos
{
    public class CustomerNotificationDto
    {
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public string ArabicTitle { get; set; } = string.Empty;
        public string EnglishTitle { get; set; } = string.Empty;
        public string ArabicMessage { get; set; } = string.Empty;
        public string EnglishMessage { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
