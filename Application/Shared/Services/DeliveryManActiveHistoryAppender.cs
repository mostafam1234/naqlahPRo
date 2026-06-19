using Domain.InterFaces;
using Domain.Models;
using System;

namespace Application.Shared.Services
{
    public static class DeliveryManActiveHistoryAppender
    {
        public static bool ApplyIfChanged(
            INaqlahContext context,
            DeliveryMan deliveryMan,
            bool newActive,
            int? changedByUserId,
            DateTime? changedAt = null)
        {
            if (deliveryMan.Active == newActive)
                return false;

            deliveryMan.ChangeActivation(newActive);
            context.DeliveryManActiveHistories.Add(
                DeliveryManActiveHistory.Create(
                    deliveryMan.Id,
                    newActive,
                    changedAt ?? DateTime.UtcNow,
                    changedByUserId));

            return true;
        }
    }
}
