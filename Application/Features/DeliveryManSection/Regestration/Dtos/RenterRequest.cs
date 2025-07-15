using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Dtos
{
    public class RenterRequest
    {
        public RenterRequest()
        {
            this.CitizenName = string.Empty;
            this.IdentityNumber = string.Empty;
            this.FrontIdentityImage = string.Empty;
            this.BackIdentityImage = string.Empty;
            this.RentContractImage = string.Empty;
            this.BankAccountNumber = string.Empty;
        }
        public string CitizenName { get;  set; }
        public string IdentityNumber { get;  set; }
        public string FrontIdentityImage { get;  set; }
        public string BackIdentityImage { get;  set; }
        public string RentContractImage { get;  set; }
        public string BankAccountNumber { get; private set; }
    }
}
