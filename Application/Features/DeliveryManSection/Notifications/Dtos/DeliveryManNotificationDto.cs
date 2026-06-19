using System;
using System.Collections.Generic;

namespace Application.Features.DeliveryManSection.Notifications.Dtos
{
    public class DeliveryManNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string BodyAr { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DeliveryManNotificationsPageDto
    {
        public List<DeliveryManNotificationDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
