using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Dtos
{
    public class EstablishMentCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string RecoredImage { get; set; } = string.Empty;
        public string TaxRegistrationNumber { get; set; } = string.Empty;
        public string TaxRegistrationImage { get; set; } = string.Empty;
        public string Address { get;  set; } = string.Empty;
        public string RepresentitveName { get; set; } = string.Empty;
        public string RepresentitvePhoneNumber { get; set; } = string.Empty;
        public string RepresentitveFrontIdentityNumberImage { get; set; } = string.Empty;
        public string RepresentitveBackIdentityNumberImage { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
