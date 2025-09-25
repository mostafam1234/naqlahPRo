using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderPackage
    {
        private OrderPackage()
        {
            this.ArabicDescripton = string.Empty;
            this.EnglishDescription = string.Empty;
        }
        public int Id { get;private set; }
        public string ArabicDescripton { get;private set; }
        public string EnglishDescription { get;private set; }
        public decimal MinWeightInKiloGram { get;private set; }
        public decimal MaxWeightInKiloGram { get;private set; }
    }
}
