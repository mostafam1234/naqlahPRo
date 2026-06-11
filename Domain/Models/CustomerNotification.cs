using System;

namespace Domain.Models
{
    public class CustomerNotification
    {
        private CustomerNotification() { }

        public int Id { get; private set; }
        public int NotificationId { get; private set; }
        public int CustomerId { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadDate { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public Notification Notification { get; private set; } = null!;

        public static CustomerNotification Instance(int customerId, DateTime createdDate)
        {
            return new CustomerNotification
            {
                CustomerId = customerId,
                IsRead = false,
                CreatedDate = createdDate
            };
        }

        public static CustomerNotification InstanceWithNotificationId(int notificationId, int customerId, DateTime createdDate)
        {
            return new CustomerNotification
            {
                NotificationId = notificationId,
                CustomerId = customerId,
                IsRead = false,
                CreatedDate = createdDate
            };
        }

        public void MarkAsRead(DateTime readDate)
        {
            IsRead = true;
            ReadDate = readDate;
        }
    }
}
