using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Dtos
{
    public class DeliveryPersonalInfoRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FrontIdenitytImage { get; set; } = string.Empty;
        public string BackIdenitytImage { get; set; } = string.Empty;
        public string PersonalImage { get; set; } = string.Empty;
        public string IdentityExpirationDate { get; set; } = string.Empty;
        public string DrivingLicenseExpirationDate { get; set; } = string.Empty;
        public int DeliveryTypeId { get; set; }
        public int DeliveryLicenseTypeId { get; set; }
        public string FrontDrivingLicenseImage { get; set; } = string.Empty;
        public string BackDrivingLicenseImage { get; set; } = string.Empty;
    }
}
