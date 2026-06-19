using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Dtos
{
    public class AddDeliveryVehicleRequest
    {
        public int VehicleTypeId { get; set; }
        public int VehicleBrandId { get; set; }

        /// <summary>Owner name (individual / company / renter) collected on the vehicle step.</summary>
        public string? VehicleOwnerName { get; set; }

        public string LicensePlateNumber { get; set; } = string.Empty;
        public string FrontImagePath { get; set; } = string.Empty;
        public string SideImagePath { get; set; } = string.Empty;
        public string FrontLicenseImagePath { get; set; } = string.Empty;
        public string? BackLicenseImagePath { get; set; }
        public string? LicenseExpirationDate { get; set; }
        public string? FrontInsuranceImagePath { get; set; }
        public string? BackInsuranceImagePath { get; set; }
        public string? InSuranceExpirationDate { get; set; }
        public int VehicleOwnerTypeId { get; set; }
    }
}
