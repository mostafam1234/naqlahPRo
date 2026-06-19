namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class CancelOrderRequestDto
    {
        /// <summary>
        /// Required only when the order has already been paid, so the refund can be issued.
        /// </summary>
        public string? Iban { get; set; }
    }

    public class CancelOrderResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RefundInitiated { get; set; }
        public string? RefundStatus { get; set; }
    }
}
