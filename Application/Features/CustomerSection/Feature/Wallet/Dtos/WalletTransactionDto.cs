using System;

namespace Application.Features.CustomerSection.Feature.Wallet.Dtos
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsWithdraw { get; set; }
        public DateTime Date { get; set; }
        public int? OrderId { get; set; }

        /// <summary>
        /// One of: charge, payment, refund, withdraw.
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
    }
}
