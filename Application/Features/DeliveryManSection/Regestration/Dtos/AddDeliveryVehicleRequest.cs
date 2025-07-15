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
        public string LicensePlateNumber { get; set; } = string.Empty;
        public string FrontImagePath { get; set; } = string.Empty;
        public string SideImagePath { get; set; } = string.Empty;
        public string FrontLicenseImagePath { get; set; } = string.Empty;
        public string BackLicenseImagePath { get; set; } = string.Empty;
        public string LicenseExpirationDate { get; set; } = string.Empty;
        public string FrontInsuranceImagePath { get; set; } = string.Empty;
        public string BackInsuranceImagePath { get; set; } = string.Empty;
        public string InSuranceExpirationDate { get; set; } = string.Empty;
        public int VehicleOwnerTypeId { get; set; }
    }
}
