using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.WalletTransactionFeatures.Dtos
{
    public class WalletTransactionAdminDto
    {
        public int Id { get; set; }
        public string ArabicDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool Withdraw { get; set; }
        public int? OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

