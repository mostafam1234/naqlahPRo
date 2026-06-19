using System.Collections.Generic;

namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public class DeliveryManActiveHistoryResponseDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool CurrentActive { get; set; }
        public string CurrentActiveStatusName { get; set; } = string.Empty;
        public List<DeliveryManActiveHistoryDto> History { get; set; } = new();
    }
}
