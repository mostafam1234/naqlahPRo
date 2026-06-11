namespace Domain.Shared
{
    public class MadaCheckoutResult
    {
        public bool Success { get; set; }
        public string CheckoutId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
