namespace Application.Features.DeliveryManSection.NewRequests.Dtos
{
    public class GetDeliveryManRequestDetailsDto
    {
        public int DeliveryManId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public DateTime? IdentityExpirationDate { get; set; }
        public DateTime? DrivingLicenseExpirationDate { get; set; }
        public string DeliveryType { get; set; } = string.Empty;
        public string DeliveryLicenseType { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;

        // --- Driver documents (NA_DeliveryMan) ---

        /// <summary>صورة هوية سائق المركبة ← FrontIdenitytImagePath</summary>
        public string? FrontIdentityImagePath { get; set; }

        /// <summary>صورة رخصة قيادة سائق المركبة ← FrontDrivingLicenseImagePath</summary>
        public string? FrontDrivingLicenseImagePath { get; set; }

        /// <summary>صورة شخصية لسائق المركبة (اختياري) ← PersonalImagePath</summary>
        public string? PersonalImagePath { get; set; }

        /// <summary>قديم — هوية خلفية ← BackIdenitytImagePath</summary>
        public string? BackIdentityImagePath { get; set; }

        /// <summary>قديم — رخصة خلفية ← BackDrivingLicenseImagePath</summary>
        public string? BackDrivingLicenseImagePath { get; set; }

        public bool Active { get; set; }
        public string AndroidDevice { get; set; } = string.Empty;
        public string IosDevice { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;

        public int? VehicleId { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? VehicleType { get; set; }
        public int? VehicleTypeId { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleBrandId { get; set; }
        public int? VehicleOwnerTypeId { get; set; }
        public string? VehicleOwnerName { get; set; }
        public string? VehicleOwnerIdentityNumber { get; set; }
        public string? CommercialRecordNumber { get; set; }
        public DateTime? VehicleLicenseExpirationDate { get; set; }
        public DateTime? VehicleInsuranceExpirationDate { get; set; }

        // --- Vehicle documents (NA_DeliveryVehicle) ---

        /// <summary>صورة أمامية للمركبة ← FrontImagePath</summary>
        public string? VehicleFrontImagePath { get; set; }

        /// <summary>صورة جانبية للمركبة ← SideImagePath</summary>
        public string? VehicleSideImagePath { get; set; }

        /// <summary>صورة رخصة سير المركبة (الاستمارة) ← FrontLicenseImagePath</summary>
        public string? VehicleFrontLicenseImagePath { get; set; }

        /// <summary>صورة تأمين المركبة (اختياري) ← FrontInsuranceImagePath</summary>
        public string? VehicleFrontInsuranceImagePath { get; set; }

        /// <summary>قديم — استمارة خلفية ← BackLicenseImagePath</summary>
        public string? VehicleBackLicenseImagePath { get; set; }

        /// <summary>قديم — تأمين خلفي ← BackInsuranceImagePath</summary>
        public string? VehicleBackInsuranceImagePath { get; set; }

        // --- Owner documents (NA_Resident / NA_Renter / NA_Company) ---

        /// <summary>صورة هوية مالك المركبة (مقيم/مستأجر) ← FrontIdentityImagePath</summary>
        public string? OwnerFrontIdentityImagePath { get; set; }

        /// <summary>صورة السجل التجاري (شركة/مؤسسة) ← RecordImagePath</summary>
        public string? CommercialRecordImagePath { get; set; }

        /// <summary>وثيقة استئجار المركبة (مستأجر — اختياري) ← RentContractImagePath</summary>
        public string? RentContractImagePath { get; set; }

        /// <summary>قديم — هوية مالك خلفية ← BackIdentityImagePath</summary>
        public string? OwnerBackIdentityImagePath { get; set; }

        public string? TaxNumber { get; set; }

        /// <summary>شهادة ضريبية (قديم — غير مطلوب في الأدمن) ← TaxCertificateImagePath</summary>
        public string? TaxCertificateImagePath { get; set; }

        public string? OwnerBankAccountNumber { get; set; }

        public bool HasIncompleteRegistration { get; set; }
    }
}
