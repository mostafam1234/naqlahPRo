namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class OrderServiceAdminDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}