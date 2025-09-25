namespace Application.Features.DeliveryManSection.Order.Dtos
{
    public sealed record ChangeWayPointStatusRequestDto
    {
        public int OrderId { get; set; }
        public int WayPointId { get; set; }
        public string PackImageBase64 { get; set; } = string.Empty;
    }
}