using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Resident
    {
        public Resident()
        {
            this.CitizenName = string.Empty;
            this.FrontIdentityImagePath = string.Empty;
            this.BackIdentityImagePath = string.Empty;
            this.BankAccountNumber = string.Empty;
        }
        [Key]
        public int DeliveryManId { get; set; }
        public string CitizenName { get; set; }
        public string FrontIdentityImagePath { get; set; }
        public string BackIdentityImagePath { get; set; }
        public string BankAccountNumber { get; set; }
    }
}
