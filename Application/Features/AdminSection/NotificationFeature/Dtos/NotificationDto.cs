namespace Application.Features.AdminSection.NotificationFeature.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public Domain.Enums.NotificationType NotificationType { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsRead { get; set; }
    }
}

