using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            this.PhoneNumber= string.Empty;

        }
        public int Id { get; set; }
        public string FullName { get;private set; }
        public string Address { get; private set; }
        public string PhoneNumber { get; private set; }
        public string IdentityNumber { get; private set; }
        public string FrontIdenitytImagePath { get; private set; }
        public string BackIdenitytImagePath { get; private set; }
        public string PersonalImagePath { get;private set; }
        public DateTime IdentityExpirationDate { get;private set; }
        public DateTime DrivingLicenseExpirationDate { get;private set; }
        public string FrontDrivingLicenseImage { get; set; }
        public DeliveryType DeliveryType { get;private set; }
        public DeliveryLicenseType DeliveryLicenseType { get;private set; }
        public string FrontDrivingLicenseImagePath { get;private set; }
        public string BackDrivingLicenseImagePath { get;private set; }

        [Required]
        public int UserId { get; private set; }
        public int? VehicleId { get;private set; }
        public User User { get;private set; }
        public DeliveryVehicle? Vehicle { get;private set; }


    }
}
