using System.Collections.Generic;

namespace Application.Features.AdminSection.DeliveryManFeature.Dtos
{
    public sealed class DeliveryManSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;

        public bool HasIncompleteRegistration { get; set; }
        public string ProfileCompletenessLabel { get; set; } = string.Empty;

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int AssignedOrders { get; set; }
        public int ConfirmedGoingToPickupOrders { get; set; }
        public int PickedUpOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ActiveOrders { get; set; }

        public List<DeliveryManOrderStatusCountDto> OrdersByStatus { get; set; } = new();
    }
}
