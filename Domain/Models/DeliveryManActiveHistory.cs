using System;

namespace Domain.Models
{
    public class DeliveryManActiveHistory
    {
        private DeliveryManActiveHistory()
        {
        }

        public int Id { get; private set; }
        public int DeliveryManId { get; private set; }
        public bool Active { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public int? ChangedByUserId { get; private set; }

        public DeliveryMan DeliveryMan { get; private set; } = null!;
        public User? ChangedByUser { get; private set; }

        public static DeliveryManActiveHistory Create(
            int deliveryManId,
            bool active,
            DateTime changedAt,
            int? changedByUserId)
        {
            return new DeliveryManActiveHistory
            {
                DeliveryManId = deliveryManId,
                Active = active,
                ChangedAt = changedAt,
                ChangedByUserId = changedByUserId
            };
        }
    }
}
