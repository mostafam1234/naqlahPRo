namespace Domain.Models
{
    public class CaptainNotification
    {
        public int Id { get; set; }
        public int DeliveryManId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string MissingFieldsJson { get; set; } = "[]";
        public string LegalDisclaimer { get; set; } = string.Empty;
        public bool IsPushSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DeliveryMan DeliveryMan { get; set; } = null!;
    }
}
