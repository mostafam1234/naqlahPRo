namespace Application.Features.CustomerSection.Feature.Payment.Dtos
{
    public class ProcessPaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>Mada/HyperPay checkout session ID — present only for paymentMethodId=1 (Mada).</summary>
        public string? CheckoutId { get; set; }

        /// <summary>URL to render the hosted payment widget — present only for paymentMethodId=1 (Mada).</summary>
        public string? CheckoutUrl { get; set; }
    }
}
