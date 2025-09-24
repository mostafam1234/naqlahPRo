using CSharpFunctionalExtensions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class DeliveryMan
    {
        public DeliveryMan()
        {
            this.FullName = string.Empty;
            this.Address = string.Empty;
            this.PhoneNumber = string.Empty;
            this.IdentityNumber = string.Empty;
            this.BackDrivingLicenseImagePath = string.Empty;
            this.FrontIdenitytImagePath = string.Empty;
            this.BackIdenitytImagePath = string.Empty;
            this.PersonalImagePath = string.Empty;
            this.FrontDrivingLicenseImagePath = string.Empty;
            this.AndriodDevice = string.Empty;
            this.IosDevice = string.Empty;
        }
        public int Id { get; set; }
        public string FullName { get; private set; }
        public string Address { get; private set; }
        public string PhoneNumber { get; private set; }
        public string IdentityNumber { get; private set; }
        public string FrontIdenitytImagePath { get; private set; }
        public string BackIdenitytImagePath { get; private set; }
        public string PersonalImagePath { get; private set; }
        public DateTime IdentityExpirationDate { get; private set; }
        public DateTime DrivingLicenseExpirationDate { get; private set; }
        public DeliveryType DeliveryType { get; private set; }
        public DeliveryRequesState DeliveryState { get; private set; }
        public DeliveryLicenseType DeliveryLicenseType { get; private set; }
        public string FrontDrivingLicenseImagePath { get; private set; }
        public string BackDrivingLicenseImagePath { get; private set; }

        [Required]
        public int UserId { get; private set; }
        public int? VehicleId { get; private set; }
        public User User { get; private set; }
        public DeliveryVehicle? Vehicle { get; private set; }
        public string AndriodDevice { get;private set; }
        public string IosDevice { get;private set; }
        public bool Active { get;private set; }
        
        public DeliveryManLocation DeliveryManLocation { get;private set; }
        private List<Assistant> _Assistants { get; set; }
        public IReadOnlyList<Assistant> Assistants
        {
            get
            {
                return _Assistants;
            }
            private set
            {
                _Assistants = (List<Assistant>)value.ToList();
            }
        }
        public static DeliveryMan Instance(string phoneNumber,
                                           string name)
        {
            var deliveryMan = new DeliveryMan
            {
                PhoneNumber = phoneNumber,
                FullName = name
            };

            return deliveryMan;
        }

        public void SaveLocation(double longitude,double latitude)
        {
            var location = this.DeliveryManLocation;
            if(location is null)
            {
                var newLocation = DeliveryManLocation.Instsance(longitude, latitude);
                this.DeliveryManLocation = newLocation;
                return;
            }

            location.UpdateLocation(longitude, latitude);
        }

        public void ChangeActivation(bool active)
        {
            this.Active = active;
        }
        public Result SetFireBaseTokens(string andriodDevice, string iosDevice)
        {
           

            this.AndriodDevice = andriodDevice;
            this.IosDevice = iosDevice;
            return Result.Success();
        }

        public Result RemoveFireBaseTokens(string andriodDevice, string iosDevice)
        {
            if (!string.IsNullOrWhiteSpace(andriodDevice))
            {
                this.AndriodDevice = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(iosDevice))
            {
                this.IosDevice = string.Empty;
            }
            return Result.Success();
        }

        public Result SetDeliveryVehicleOwnerAsResident(string citizenName,
                                                        string IdentityNumber,
                                                        string frontIdentityImage,
                                                        string backIdentityImage,
                                                        string bankAccountNumber)
        {
            var vehicle = this.Vehicle;
            if (vehicle is null)
            {
                return Result.Failure("DeliveryMan Should Have vehicle");
            }
            vehicle.SetCarOwnerAsResident(citizenName,
                                          IdentityNumber,
                                          frontIdentityImage,
                                          backIdentityImage,
                                          bankAccountNumber);

            return Result.Success();
        }


        public Result SetDeliveryVehicleOwnerAsRenter(string citizenName,
                                                      string identityNumber,
                                                      string frontIdentityImagePath,
                                                      string backIdentityImagePath,
                                                      string rentContractImagePath,
                                                      string bankAccountNumber)
        {
            var vehicle = this.Vehicle;
            if (vehicle is null)
            {
                return Result.Failure("DeliveryMan Should Have vehicle");
            }
            vehicle.SetCarOwnerAsRenter(citizenName,
                                        identityNumber,
                                        frontIdentityImagePath,
                                        backIdentityImagePath,
                                        rentContractImagePath,
                                        bankAccountNumber);

            return Result.Success();
        }
        public Result SetDeliveryVehicleOwnerAsCompany(string companName,
                                                       string commercialRecordNumbeer,
                                                        string recordImagePath,
                                                        string taxNumber,
                                                        string taxCertificateImagePath,
                                                        string bankAccounNumber)
        {
            var vehicle = this.Vehicle;
            if (vehicle is null)
            {
                return Result.Failure("DeliveryMan Should Have vehicle");
            }
            vehicle.SetCarOwnerAsCompany(companName,
                                         commercialRecordNumbeer,
                                         recordImagePath,
                                         taxNumber,
                                         taxCertificateImagePath,
                                         bankAccounNumber);

            return Result.Success();
        }

        public Result UpdatePersnalInfo(string fullName,
                                        string address,
                                        string IdentityNumber,
                                        string frontIdenitytImage,
                                        string backIdentityImage,
                                        string personalImage,
                                        DateTime identityExpirationDate,
                                        DateTime DrivingLicenseExpirationDate,
                                        int deliveryTypeId,
                                        int deliveryLicenseTypeId,
                                        string frontDrivingLicenseImage,
                                        string backDrivingLicenseImage
                                        )
        {
            var deliveryType = (DeliveryType)deliveryTypeId;
            var licenseType = (DeliveryLicenseType)deliveryLicenseTypeId;

            this.FullName = fullName;
            this.Address = address;
            this.IdentityNumber = IdentityNumber;
            this.FrontIdenitytImagePath = frontIdenitytImage;
            this.BackIdenitytImagePath = backIdentityImage;
            this.PersonalImagePath = personalImage;
            this.IdentityExpirationDate = identityExpirationDate;
            this.DrivingLicenseExpirationDate = DrivingLicenseExpirationDate;
            this.FrontDrivingLicenseImagePath = frontDrivingLicenseImage;
            this.BackDrivingLicenseImagePath = backDrivingLicenseImage;
            this.DeliveryType = deliveryType;
            this.DeliveryLicenseType = licenseType;
            return Result.Success();
        }

        public Result AddVehicle(int vehicleTypeId,
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
                                 int vehicleOwnerTypeId)
        {
            var vehicle = DeliveryVehicle.Instance(vehicleTypeId,
                                                 vehicleBrandId,
                                                 licensePlateNumber,
                                                 frontImagePath,
                                                 sideImagePath,
                                                 frontLicenseImagePath,
                                                 backLicenseImagePath,
                                                 licenseExpirationDate,
                                                 frontInsuranceImagePath,
                                                 backInsuranceImagePath,
                                                 inSuranceExpirationDate,
                                                 vehicleOwnerTypeId);

            this.Vehicle = vehicle.Value;
            return Result.Success();
        }

        public void UpdateDeliveryManRequestState(int state)
        {
            if (Enum.IsDefined(typeof(DeliveryRequesState), state))
            {
                this.DeliveryState = (DeliveryRequesState)state;
            }
            else
            {
                throw new ArgumentException("Invalid state value.", nameof(state));
            }
        }

        public static DeliveryMan CreateFullInstance(
            string fullName,
            string address,
            string phoneNumber,
            string identityNumber,
            string personalImagePath,
            DeliveryType deliveryType,
            DeliveryRequesState state = DeliveryRequesState.New)
        {
            var deliveryMan = new DeliveryMan
            {
                FullName = fullName,
                Address = address,
                PhoneNumber = phoneNumber,
                IdentityNumber = identityNumber,
                PersonalImagePath = personalImagePath,
                DeliveryType = deliveryType,
                DeliveryState = state,
                Active = true
            };

            return deliveryMan;
        }
    }
}
