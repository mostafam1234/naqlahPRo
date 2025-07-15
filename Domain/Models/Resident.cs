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
            this.IdentityNumber = string.Empty;
        }
        [Key]
        public int DeliveryVehicleId { get;private set; }
        public string CitizenName { get;private set; }
        public string IdentityNumber { get;private set; }
        public string FrontIdentityImagePath { get;private set; }
        public string BackIdentityImagePath { get;private set; }
        public string BankAccountNumber { get;private set; }
    }
}
