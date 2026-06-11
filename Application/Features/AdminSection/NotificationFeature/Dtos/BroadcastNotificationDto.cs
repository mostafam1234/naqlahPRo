using Domain.Enums;
using System;

namespace Application.Features.AdminSection.NotificationFeature.Dtos
{
    public class BroadcastNotificationDto
    {
        public int Id { get; set; }
        public string ArabicTitle { get; set; } = string.Empty;
        public string EnglishTitle { get; set; } = string.Empty;
        public string ArabicMessage { get; set; } = string.Empty;
        public string EnglishMessage { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public bool TargetAll { get; set; }
        public int? TargetCustomerType { get; set; }
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public bool IsProcessed { get; set; }
        public int RecipientCount { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
