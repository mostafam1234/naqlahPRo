namespace Application.Features.CustomerSection.Feature.Payment.Dtos
{
    public class PayFromWalletResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
    }
}
