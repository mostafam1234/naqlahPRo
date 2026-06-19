using System;

namespace Application.Features.DeliveryManSection.Wallet.Dtos
{
    public class DeliveryManWalletBalanceDto
    {
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
    }

    public class DeliveryManWalletTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        /// <summary>credit | debit</summary>
        public string Type { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DeliveryManWithdrawResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
    }
}
