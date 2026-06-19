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

        /// <summary>One of: newOrder, orderReassigned, incompleteRegistration, pickupReminder, general.</summary>
        public string Type { get; set; } = "general";
        public int? OrderId { get; set; }
        public bool IsRead { get; set; }
        public System.DateTime? ReadDate { get; set; }

        public System.DateTime CreatedAt { get; set; }
        public DeliveryMan DeliveryMan { get; set; } = null!;

        public void MarkAsRead(System.DateTime nowDate)
        {
            if (this.IsRead)
            {
                return;
            }

            this.IsRead = true;
            this.ReadDate = nowDate;
        }
    }
}
