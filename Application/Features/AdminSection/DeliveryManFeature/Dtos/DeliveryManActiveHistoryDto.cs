using System;

namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public class DeliveryManActiveHistoryDto
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public string ActiveStatusName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
    }
}
