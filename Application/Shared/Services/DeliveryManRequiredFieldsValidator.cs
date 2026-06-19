using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.Models;

namespace Application.Shared.Services
{
    public static class DeliveryManRequiredFieldsValidator
    {
        public static Result ValidateForAdminCreate(AddDeliveryManDto dto)
        {
            var basicValidation = ValidateBasicFields(dto, requireAccountFields: true);
            if (basicValidation.IsFailure)
                return basicValidation;

            return ValidateRequiredDocuments(dto, existing: null, validateVehicle: true);
        }

        public static Result ValidateForAdminUpdate(AddDeliveryManDto dto, DeliveryMan existing)
        {
            var validateVehicle = dto.VehicleTypeId.HasValue &&
                                  dto.VehicleBrandId.HasValue &&
                                  !string.IsNullOrWhiteSpace(dto.VehiclePlateNumber);

            return ValidateRequiredDocuments(dto, existing, validateVehicle);
        }

        private static Result ValidateBasicFields(AddDeliveryManDto dto, bool requireAccountFields)
        {
            if (requireAccountFields)
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return Result.Failure("EmailRequired");

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return Result.Failure("PasswordRequired");
            }

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return Result.Failure("DriverNameRequired");

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return Result.Failure("PhoneNumberRequired");

            if (string.IsNullOrWhiteSpace(dto.IdentityNumber))
                return Result.Failure("IdentityNumberRequired");

            if (string.IsNullOrWhiteSpace(dto.BirthDate))
                return Result.Failure("BirthDateRequired");

            if (!dto.DeliveryType.HasValue || !Enum.IsDefined(typeof(DeliveryType), dto.DeliveryType.Value))
                return Result.Failure("DeliveryTypeRequired");

            if (dto.DeliveryLicenseType <= 0)
                return Result.Failure("DeliveryLicenseTypeRequired");

            return Result.Success();
        }

        private static Result ValidateRequiredDocuments(
            AddDeliveryManDto dto,
            DeliveryMan? existing,
            bool validateVehicle)
        {
            if (!HasImageValue(dto.FrontIdentityImagePath, existing?.FrontIdenitytImagePath))
                return Result.Failure("DriverIdentityPhotoRequired");

            if (!HasImageValue(dto.FrontDrivingLicenseImagePath, existing?.FrontDrivingLicenseImagePath))
                return Result.Failure("DriverDrivingLicensePhotoRequired");

            if (!validateVehicle)
                return Result.Success();

            if (!dto.VehicleTypeId.HasValue || dto.VehicleTypeId <= 0)
                return Result.Failure("VehicleTypeRequired");

            if (!dto.VehicleBrandId.HasValue || dto.VehicleBrandId <= 0)
                return Result.Failure("VehicleBrandRequired");

            if (string.IsNullOrWhiteSpace(dto.VehiclePlateNumber))
                return Result.Failure("VehiclePlateNumberRequired");

            if (!dto.VehicleOwnerTypeId.HasValue || dto.VehicleOwnerTypeId <= 0)
                return Result.Failure("VehicleOwnerTypeRequired");

            if (string.IsNullOrWhiteSpace(dto.VehicleOwnerName))
                return Result.Failure("VehicleOwnerNameRequired");

            var vehicle = existing?.Vehicle;

            if (!HasImageValue(dto.VehicleFrontImagePath, vehicle?.FrontImagePath))
                return Result.Failure("VehicleFrontPhotoRequired");

            if (!HasImageValue(dto.VehicleSideImagePath, vehicle?.SideImagePath))
                return Result.Failure("VehicleSidePhotoRequired");

            if (!HasImageValue(dto.VehicleFrontLicenseImagePath, vehicle?.FrontLicenseImagePath))
                return Result.Failure("VehicleRegistrationPhotoRequired");

            var ownerType = (VehicleOwnerType)dto.VehicleOwnerTypeId.Value;

            if (ownerType is VehicleOwnerType.Resident or VehicleOwnerType.Renter)
            {
                var existingOwnerFront = ownerType == VehicleOwnerType.Resident
                    ? vehicle?.Resident?.FrontIdentityImagePath
                    : vehicle?.Renter?.FrontIdentityImagePath;

                if (!HasImageValue(dto.OwnerFrontIdentityImagePath, existingOwnerFront))
                    return Result.Failure("VehicleOwnerIdentityPhotoRequired");
            }
            else if (ownerType == VehicleOwnerType.Company)
            {
                if (!HasImageValue(dto.CommercialRecordImagePath, vehicle?.Company?.RecordImagePath))
                    return Result.Failure("CommercialRegisterPhotoRequired");
            }

            return Result.Success();
        }

        private static bool HasImageValue(string? incoming, string? existing) =>
            !string.IsNullOrWhiteSpace(incoming) || !string.IsNullOrWhiteSpace(existing);
    }
}
