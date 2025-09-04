using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class PaymentMethod
    {
        public PaymentMethod()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this.Active = true;
        }
        public int Id { get;private set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public bool Active { get; set; }
    }
}
