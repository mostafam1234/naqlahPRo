namespace Application.Features.DeliveryManSection.Wallet.Dtos
{
    public class DeliveryManWithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public int BankAccountId { get; set; }
    }
}
