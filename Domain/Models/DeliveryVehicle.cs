using CSharpFunctionalExtensions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class DeliveryVehicle
    {
        public DeliveryVehicle()
        {
            this.LicensePlateNumber = string.Empty;
            this.FrontImagePath=string.Empty;
            this.FrontLicenseImagePath = string.Empty;
            this.SideImagePath = string.Empty;
            this.FrontLicenseImagePath = string.Empty;
            this.BackLicenseImagePath = string.Empty;
            this.BackInsuranceImagePath = string.Empty;
            this.FrontInsuranceImagePath = string.Empty;
        }
        public int Id { get;private set; }

        [Required]
        public int DeliveryManId { get;private set; }
        public int VehicleTypeId { get;private set; }
        public int VehicleBrandId { get;private set; }
        public string LicensePlateNumber { get;private set; }
        public string FrontImagePath { get;private set; }
        public string SideImagePath { get;private set; }
        public string FrontLicenseImagePath { get;private set; }
        public string BackLicenseImagePath { get;private set; }
        public DateTime LicenseExpirationDate { get;private set; }
        public string FrontInsuranceImagePath { get; private set; }
        public string BackInsuranceImagePath { get; private set; }
        public DateTime InSuranceExpirationDate { get;private set; }
        public VehicleOwnerType VehicleOwnerType { get;private set; }
        public Resident? Resident { get;private set; }
        public Company? Company { get;private set; }
        public Renter? Renter { get;private set; }
        public DeliveryMan DeliveryMan { get;private set; }
        public VehicleType VehicleType { get;private set; }
        public VehicleBrand VehicleBrand { get;private set; }


        public static Result<DeliveryVehicle>Instance(int vehicleTypeId,
                                                      int vehicleBrandId,
                                                      string licensePlateNumber,
                                                      string frontImagePath,
                                                      string sideImagePath,
                                                      string frontLicenseImagePath,
                                                      string backLicenseImagePath,
                                                      DateTime licenseExpirationDate,
                                                      string frontInsuranceImagePath,
                                                      string backInsuranceImagePath,
                                                      DateTime inSuranceExpirationDate,
                                                      int vehicleOwnerTypeId
                                                      )
        {
            var vehicle = new DeliveryVehicle
            {
                VehicleBrandId = vehicleBrandId,
                VehicleTypeId = vehicleTypeId,
                LicensePlateNumber = licensePlateNumber,
                LicenseExpirationDate = licenseExpirationDate,
                FrontImagePath = frontImagePath,
                SideImagePath = sideImagePath,
                BackInsuranceImagePath = backInsuranceImagePath,
                FrontInsuranceImagePath = frontInsuranceImagePath,
                FrontLicenseImagePath = frontLicenseImagePath,
                BackLicenseImagePath = backLicenseImagePath,
                InSuranceExpirationDate = inSuranceExpirationDate,

            };

            return vehicle;
        }
    }
}
