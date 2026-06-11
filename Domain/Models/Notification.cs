using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Notification
    {
        private readonly List<CustomerNotification> _customerNotifications = new();

        private Notification()
        {
            this.ArabicTitle = string.Empty;
            this.EnglishTitle = string.Empty;
            this.ArabicMessage = string.Empty;
            this.EnglishMessage = string.Empty;
        }

        public int Id { get; private set; }
        public string ArabicTitle { get; private set; }
        public string EnglishTitle { get; private set; }
        public string ArabicMessage { get; private set; }
        public string EnglishMessage { get; private set; }
        public int? OrderId { get; private set; }
        public NotificationType NotificationType { get; private set; }
        public DateTime CreationDate { get; private set; }
        public bool IsRead { get; private set; }
        public int? UserId { get; private set; }
        public bool IsScheduled { get; private set; }
        public DateTime? ScheduledDate { get; private set; }
        public bool TargetAll { get; private set; }
        public int? TargetCustomerType { get; private set; }
        public bool IsProcessed { get; private set; }
        public bool IsAdminBroadcast { get; private set; }
        public IReadOnlyList<CustomerNotification> CustomerNotifications => _customerNotifications;

        public static Notification Create(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            DateTime now,
            int? orderId = null,
            int? userId = null)
        {
            return new Notification
            {
                ArabicTitle = arabicTitle,
                EnglishTitle = englishTitle,
                ArabicMessage = arabicMessage,
                EnglishMessage = englishMessage,
                NotificationType = notificationType,
                OrderId = orderId,
                UserId = userId,
                CreationDate = now,
                IsRead = false
            };
        }

        public static Notification CreateForCustomers(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            DateTime now,
            bool targetAll,
            int? targetCustomerType,
            bool isScheduled,
            DateTime? scheduledDate,
            bool isProcessed)
        {
            return new Notification
            {
                ArabicTitle = arabicTitle,
                EnglishTitle = englishTitle,
                ArabicMessage = arabicMessage,
                EnglishMessage = englishMessage,
                NotificationType = notificationType,
                CreationDate = now,
                IsRead = false,
                TargetAll = targetAll,
                TargetCustomerType = targetCustomerType,
                IsScheduled = isScheduled,
                ScheduledDate = scheduledDate,
                IsProcessed = isProcessed,
                IsAdminBroadcast = true
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
        }

        public void AddCustomerNotification(int customerId, DateTime createdDate)
        {
            _customerNotifications.Add(CustomerNotification.Instance(customerId, createdDate));
        }
    }
}
