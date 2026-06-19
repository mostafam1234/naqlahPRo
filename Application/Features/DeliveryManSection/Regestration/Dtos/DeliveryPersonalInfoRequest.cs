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
        public string? Address { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>Format: yyyy-MM-dd. Replaces the deprecated resident/citizen toggle.</summary>
        public string? DateOfBirth { get; set; }

        public string FrontIdenitytImage { get; set; } = string.Empty;
        public string? BackIdenitytImage { get; set; }
        public string? PersonalImage { get; set; }
        public string? IdentityExpirationDate { get; set; }
        public string? DrivingLicenseExpirationDate { get; set; }

        /// <summary>Deprecated — app no longer sends this. Kept optional for backward compatibility.</summary>
        public int? DeliveryTypeId { get; set; }

        public int DeliveryLicenseTypeId { get; set; }
        public string FrontDrivingLicenseImage { get; set; } = string.Empty;
        public string? BackDrivingLicenseImage { get; set; }
    }
}
