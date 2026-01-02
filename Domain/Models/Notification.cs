using Domain.Enums;
using System;

namespace Domain.Models
{
    public class Notification
    {
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
                CreationDate = DateTime.UtcNow,
                IsRead = false
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}

