using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Wallet.Dtos
{
    public class CustomerWalletBalanceDto
    {
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public int TransactionCount { get; set; }
    }
}