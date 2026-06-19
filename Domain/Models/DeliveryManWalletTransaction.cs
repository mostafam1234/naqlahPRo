using CSharpFunctionalExtensions;
using System;

namespace Domain.Models
{
    public class DeliveryManWalletTransaction
    {
        public const string StatusCompleted = "completed";
        public const string StatusPending = "pending";

        private DeliveryManWalletTransaction()
        {
            this.Description = string.Empty;
            this.DescriptionAr = string.Empty;
            this.Status = StatusCompleted;
        }

        public int Id { get; private set; }
        public int DeliveryManId { get; private set; }
        public decimal Amount { get; private set; }

        /// <summary>true = credit (payout), false = debit (withdrawal/charge).</summary>
        public bool IsCredit { get; private set; }
        public string Description { get; private set; }
        public string DescriptionAr { get; private set; }
        public string Status { get; private set; }
        public int? OrderId { get; private set; }
        public int? BankAccountId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public DeliveryMan DeliveryMan { get; private set; } = null!;

        public static Result<DeliveryManWalletTransaction> Create(int deliveryManId,
                                                                  decimal amount,
                                                                  bool isCredit,
                                                                  string description,
                                                                  string descriptionAr,
                                                                  string status,
                                                                  DateTime createdAt,
                                                                  int? orderId = null,
                                                                  int? bankAccountId = null)
        {
            if (amount <= 0)
            {
                return Result.Failure<DeliveryManWalletTransaction>("Amount must be greater than zero");
            }

            var transaction = new DeliveryManWalletTransaction
            {
                DeliveryManId = deliveryManId,
                Amount = amount,
                IsCredit = isCredit,
                Description = description ?? string.Empty,
                DescriptionAr = descriptionAr ?? string.Empty,
                Status = string.IsNullOrWhiteSpace(status) ? StatusCompleted : status,
                CreatedAt = createdAt,
                OrderId = orderId,
                BankAccountId = bankAccountId
            };

            return Result.Success(transaction);
        }

        public void MarkCompleted()
        {
            this.Status = StatusCompleted;
        }
    }
}
