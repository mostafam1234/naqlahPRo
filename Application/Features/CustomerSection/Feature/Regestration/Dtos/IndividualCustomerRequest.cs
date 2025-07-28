using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Dtos
{
    public class IndividualCustomerRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FrontIdentityImage { get; set; } = string.Empty;
        public string BackIdentityImage { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
