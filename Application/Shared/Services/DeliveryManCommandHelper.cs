using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using System.Globalization;

namespace Application.Shared.Services
{
    public static class DeliveryManCommandHelper
    {
        public static bool TryParseOptionalDate(string? value, out DateTime? date)
        {
            date = null;
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                date = parsed;
                return true;
            }

            return false;
        }

        public static async Task ApplyVehicleOwnerAsync(
            DeliveryMan deliveryMan,
            AddDeliveryManDto dto,
            IMediaUploader mediaUploader,
            string deliveryFolder)
        {
            if (deliveryMan.Vehicle is null || !dto.VehicleOwnerTypeId.HasValue)
                return;

            var ownerType = (VehicleOwnerType)dto.VehicleOwnerTypeId.Value;
            var ownerName = dto.VehicleOwnerName ?? string.Empty;
            var ownerIdentityNumber = dto.VehicleOwnerIdentityNumber ?? string.Empty;

            var ownerFrontImage = string.Empty;
            if (!string.IsNullOrWhiteSpace(dto.OwnerFrontIdentityImagePath))
            {
                ownerFrontImage = await mediaUploader.UploadFromBase64(dto.OwnerFrontIdentityImagePath, deliveryFolder);
            }

            string? ownerBackImage = null;
            if (!string.IsNullOrWhiteSpace(dto.OwnerBackIdentityImagePath))
            {
                ownerBackImage = await mediaUploader.UploadFromBase64(dto.OwnerBackIdentityImagePath, deliveryFolder);
            }

            switch (ownerType)
            {
                case VehicleOwnerType.Resident:
                    deliveryMan.SetDeliveryVehicleOwnerAsResident(
                        ownerName,
                        ownerIdentityNumber,
                        ownerFrontImage,
                        ownerBackImage,
                        dto.OwnerBankAccountNumber);
                    break;
                case VehicleOwnerType.Company:
                    var recordImage = string.Empty;
                    if (!string.IsNullOrWhiteSpace(dto.CommercialRecordImagePath))
                    {
                        recordImage = await mediaUploader.UploadFromBase64(dto.CommercialRecordImagePath, deliveryFolder);
                    }
                    deliveryMan.SetDeliveryVehicleOwnerAsCompany(
                        ownerName,
                        dto.CommercialRecordNumber ?? string.Empty,
                        recordImage,
                        dto.TaxNumber ?? string.Empty,
                        string.Empty,
                        dto.OwnerBankAccountNumber ?? string.Empty);
                    break;
                case VehicleOwnerType.Renter:
                    string? rentContract = null;
                    if (!string.IsNullOrWhiteSpace(dto.RentContractImagePath))
                    {
                        rentContract = await mediaUploader.UploadFromBase64(dto.RentContractImagePath, deliveryFolder);
                    }
                    deliveryMan.SetDeliveryVehicleOwnerAsRenter(
                        ownerName,
                        ownerIdentityNumber,
                        ownerFrontImage,
                        ownerBackImage,
                        rentContract,
                        dto.OwnerBankAccountNumber);
                    break;
            }
        }

        public static void RefreshCompleteness(DeliveryMan deliveryMan)
        {
            DeliveryManProfileCompleteness.ApplyCompletenessState(deliveryMan);
        }
    }
}
