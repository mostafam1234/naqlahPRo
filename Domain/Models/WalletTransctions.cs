using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class WalletTransctions
    {
        public int Id { get;private set; }
        public string ArabicDescription { get;private set; }
        public string EnglishDescription { get;private set; }
        public decimal Amount { get;private set; }
        public bool Withdraw { get;private set; }
        public int? OrderId { get;private set; }
        public int CustomerId { get;private set; }
    }
}
