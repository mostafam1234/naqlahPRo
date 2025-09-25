using Domain.Enums;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos
{
    public class AddDeliveryManDto
    {
        // Basic Personal Information
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public int DeliveryType { get; set; }
        
        // Identity Information
        public string IdentityExpirationDate { get; set; } = string.Empty;

        // Driving License Information
        public int DeliveryLicenseType { get; set; }
        public string DrivingLicenseExpirationDate { get; set; } = string.Empty;
        
    // Vehicle Information
    public string? VehiclePlateNumber { get; set; }
    public int? VehicleTypeId { get; set; }
    public int? VehicleBrandId { get; set; }
    public int? VehicleOwnerTypeId { get; set; }
        
    // Vehicle Dates
    public string? VehicleLicenseExpirationDate { get; set; }
    public string? VehicleInsuranceExpirationDate { get; set; }
    public string? InSuranceExpirationDate { get; set; }
        
    // Vehicle Images
    public string? VehicleFrontImagePath { get; set; }
    public string? VehicleSideImagePath { get; set; }
    public string? VehicleFrontLicenseImagePath { get; set; }
    public string? VehicleBackLicenseImagePath { get; set; }
    public string? VehicleFrontInsuranceImagePath { get; set; }
    public string? VehicleBackInsuranceImagePath { get; set; }
        
        // Image Paths
        public string? PersonalImagePath { get; set; }
        public string? FrontIdentityImagePath { get; set; }
        public string? BackIdentityImagePath { get; set; }
        public string? FrontDrivingLicenseImagePath { get; set; }
        public string? BackDrivingLicenseImagePath { get; set; }
        
        // Device Information
        public string? AndroidDevice { get; set; }
        public string? IosDevice { get; set; }
        
        // Status
        public bool Active { get; set; } = true;
    }
}