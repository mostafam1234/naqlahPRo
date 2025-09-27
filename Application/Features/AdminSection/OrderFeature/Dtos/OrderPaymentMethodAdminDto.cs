using Domain.Enums;

namespace Application.Features.AdminSection.OrderFeature.Dtos
{
    public class OrderPaymentMethodAdminDto
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public OrderPaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusName { get; set; } = string.Empty;
    }
}