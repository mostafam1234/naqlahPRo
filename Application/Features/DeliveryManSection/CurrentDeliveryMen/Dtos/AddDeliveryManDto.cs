namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos
{
    public class AddDeliveryManDto
    {
        // User Account Information
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Basic Personal Information
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? BirthDate { get; set; }
        public int? DeliveryType { get; set; }
        
        // Identity Information (optional)
        public string? IdentityExpirationDate { get; set; }

        // Driving License Information
        public int DeliveryLicenseType { get; set; }
        public string? DrivingLicenseExpirationDate { get; set; }
        
        // Vehicle Information
        public string? VehiclePlateNumber { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? VehicleBrandId { get; set; }
        public int? VehicleOwnerTypeId { get; set; }
        public string? VehicleOwnerName { get; set; }
        public string? VehicleOwnerIdentityNumber { get; set; }
        public string? OwnerBankAccountNumber { get; set; }
        public string? CommercialRecordNumber { get; set; }
        public string? TaxNumber { get; set; }
        
        // Vehicle Dates (optional)
        public string? VehicleLicenseExpirationDate { get; set; }
        public string? VehicleInsuranceExpirationDate { get; set; }

        // --- Required documents ---

        /// <summary>صورة هوية سائق المركبة → NA_DeliveryMan.FrontIdenitytImagePath</summary>
        public string? FrontIdentityImagePath { get; set; }

        /// <summary>صورة رخصة قيادة سائق المركبة → NA_DeliveryMan.FrontDrivingLicenseImagePath</summary>
        public string? FrontDrivingLicenseImagePath { get; set; }

        /// <summary>صورة هوية مالك المركبة (مقيم/مستأجر) → NA_Resident/NA_Renter.FrontIdentityImagePath</summary>
        public string? OwnerFrontIdentityImagePath { get; set; }

        /// <summary>صورة السجل التجاري (شركة/مؤسسة) → NA_Company.RecordImagePath</summary>
        public string? CommercialRecordImagePath { get; set; }

        /// <summary>صورة أمامية للمركبة → NA_DeliveryVehicle.FrontImagePath</summary>
        public string? VehicleFrontImagePath { get; set; }

        /// <summary>صورة جانبية للمركبة → NA_DeliveryVehicle.SideImagePath</summary>
        public string? VehicleSideImagePath { get; set; }

        /// <summary>صورة رخصة سير المركبة (الاستمارة) → NA_DeliveryVehicle.FrontLicenseImagePath</summary>
        public string? VehicleFrontLicenseImagePath { get; set; }

        // --- Optional documents ---

        /// <summary>صورة شخصية لسائق المركبة → NA_DeliveryMan.PersonalImagePath</summary>
        public string? PersonalImagePath { get; set; }

        /// <summary>صورة تأمين المركبة → NA_DeliveryVehicle.FrontInsuranceImagePath</summary>
        public string? VehicleFrontInsuranceImagePath { get; set; }

        /// <summary>وثيقة استئجار المركبة (مستأجر) → NA_Renter.RentContractImagePath</summary>
        public string? RentContractImagePath { get; set; }

        // --- Legacy (mobile only — admin sends null) ---

        /// <summary>قديم → NA_DeliveryMan.BackIdenitytImagePath</summary>
        public string? BackIdentityImagePath { get; set; }

        /// <summary>قديم → NA_DeliveryMan.BackDrivingLicenseImagePath</summary>
        public string? BackDrivingLicenseImagePath { get; set; }

        /// <summary>قديم → NA_DeliveryVehicle.BackLicenseImagePath</summary>
        public string? VehicleBackLicenseImagePath { get; set; }

        /// <summary>قديم → NA_DeliveryVehicle.BackInsuranceImagePath</summary>
        public string? VehicleBackInsuranceImagePath { get; set; }

        /// <summary>قديم → NA_Resident/NA_Renter.BackIdentityImagePath</summary>
        public string? OwnerBackIdentityImagePath { get; set; }
        
        // Device Information
        public string? AndroidDevice { get; set; }
        public string? IosDevice { get; set; }
        
        // Status
        public bool Active { get; set; } = true;
    }
}
