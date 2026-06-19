using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands
{
    public class UpdateDeliveryManCommandHandler : IRequestHandler<UpdateDeliveryManCommand, Result<int>>
    {
        private readonly INaqlahContext _context;
        private readonly IMediaUploader mediaUploader;
        private readonly UserManager<User> _userManager;
        private const string DeliveryFolderPrefix = "DeliveryMan";

        public UpdateDeliveryManCommandHandler(
            INaqlahContext context,
            IMediaUploader mediaUploader,
            UserManager<User> userManager)
        {
            _context = context;
            this.mediaUploader = mediaUploader;
            this._userManager = userManager;
        }

        public async Task<Result<int>> Handle(UpdateDeliveryManCommand request, CancellationToken cancellationToken)
        {
            var deliveryMan = await _context.DeliveryMen
                .Include(x => x.User)
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Resident)
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Company)
                .Include(x => x.Vehicle!)
                    .ThenInclude(v => v.Renter)
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == request.DeliveryManId, cancellationToken);

            if (deliveryMan == null)
                return Result.Failure<int>("DeliveryManNotFound");

            var requiredValidation = DeliveryManRequiredFieldsValidator.ValidateForAdminUpdate(request.DeliveryMan, deliveryMan);
            if (requiredValidation.IsFailure)
                return Result.Failure<int>(requiredValidation.Error);

            if (!string.IsNullOrWhiteSpace(request.DeliveryMan.Email) && deliveryMan.User != null)
            {
                var user = deliveryMan.User;
                if (user.Email != request.DeliveryMan.Email)
                {
                    user.Email = request.DeliveryMan.Email;
                    user.NormalizedEmail = request.DeliveryMan.Email.ToUpperInvariant();
                    user.UserName = request.DeliveryMan.Email;
                    user.NormalizedUserName = request.DeliveryMan.Email.ToUpperInvariant();
                }

                if (!string.IsNullOrWhiteSpace(request.DeliveryMan.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.DeliveryMan.Password);
                    if (!passwordResult.Succeeded)
                        return Result.Failure<int>("FailedToUpdatePassword");
                }
            }

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.IdentityExpirationDate, out var identityExpirationDate))
                return Result.Failure<int>("InvalidIdentityExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.DrivingLicenseExpirationDate, out var licenceExpirationDate))
                return Result.Failure<int>("InvalidDrivingLicenseExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.BirthDate, out var birthDate))
                return Result.Failure<int>("InvalidBirthDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.VehicleLicenseExpirationDate, out var vehicleLicenseExpirationDate))
                return Result.Failure<int>("InvalidVehicleLicenseExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.VehicleInsuranceExpirationDate, out var vehicleInsuranceExpirationDate))
                return Result.Failure<int>("InvalidVehicleInsuranceExpirationDateFormat");

            var deliveryFolder = $"{DeliveryFolderPrefix}_{deliveryMan.Id}";

            bool IsBase64Image(string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                return value.StartsWith("data:image/") || (value.Length > 100 && !value.StartsWith("http"));
            }

            string ResolveImage(string? incoming, string current)
            {
                if (string.IsNullOrEmpty(incoming))
                    return current;
                if (IsBase64Image(incoming))
                    return incoming;
                if (incoming.Contains("/ImageBank/"))
                    return incoming.Split('/').Last();
                return current;
            }

            async Task<string?> UploadIfBase64Async(string? incoming, string? current)
            {
                if (string.IsNullOrEmpty(incoming))
                    return current;
                if (IsBase64Image(incoming))
                    return await mediaUploader.UploadFromBase64(incoming, deliveryFolder);
                return ResolveImage(incoming, current ?? string.Empty);
            }

            async Task<string?> UploadOrClearDeprecatedAsync(string? incoming)
            {
                if (string.IsNullOrEmpty(incoming))
                    return null;
                if (IsBase64Image(incoming))
                    return await mediaUploader.UploadFromBase64(incoming, deliveryFolder);
                if (incoming.Contains("/ImageBank/"))
                    return incoming.Split('/').Last();
                return null;
            }

            var frontIdenitytImagePath = await UploadIfBase64Async(request.DeliveryMan.FrontIdentityImagePath, deliveryMan.FrontIdenitytImagePath) ?? deliveryMan.FrontIdenitytImagePath;
            var backIdenitytImagePath = await UploadOrClearDeprecatedAsync(request.DeliveryMan.BackIdentityImagePath);
            var personalImagePath = await UploadIfBase64Async(request.DeliveryMan.PersonalImagePath, deliveryMan.PersonalImagePath);
            var frontLicenseImagePath = await UploadIfBase64Async(request.DeliveryMan.FrontDrivingLicenseImagePath, deliveryMan.FrontDrivingLicenseImagePath) ?? deliveryMan.FrontDrivingLicenseImagePath;
            var backLicenseImagePath = await UploadOrClearDeprecatedAsync(request.DeliveryMan.BackDrivingLicenseImagePath);

            var resolvedBirthDate = birthDate ?? deliveryMan.BirthDate;
            var resolvedIdentityExpiry = identityExpirationDate ?? deliveryMan.IdentityExpirationDate;
            var resolvedLicenseExpiry = licenceExpirationDate ?? deliveryMan.DrivingLicenseExpirationDate;

            var updateResult = deliveryMan.UpdatePersnalInfo(
                string.IsNullOrWhiteSpace(request.DeliveryMan.FullName) ? deliveryMan.FullName : request.DeliveryMan.FullName,
                request.DeliveryMan.Address ?? deliveryMan.Address,
                string.IsNullOrWhiteSpace(request.DeliveryMan.IdentityNumber) ? deliveryMan.IdentityNumber : request.DeliveryMan.IdentityNumber,
                frontIdenitytImagePath,
                backIdenitytImagePath,
                personalImagePath,
                resolvedIdentityExpiry,
                resolvedLicenseExpiry,
                resolvedBirthDate,
                request.DeliveryMan.DeliveryType > 0 ? request.DeliveryMan.DeliveryType : (int)deliveryMan.DeliveryType,
                request.DeliveryMan.DeliveryLicenseType > 0 ? request.DeliveryMan.DeliveryLicenseType : (int)deliveryMan.DeliveryLicenseType,
                frontLicenseImagePath,
                backLicenseImagePath
            );

            if (updateResult.IsFailure)
                return Result.Failure<int>(updateResult.Error);

            if (request.DeliveryMan.VehicleTypeId.HasValue &&
                request.DeliveryMan.VehicleBrandId.HasValue &&
                !string.IsNullOrWhiteSpace(request.DeliveryMan.VehiclePlateNumber))
            {
                var ownerTypeId = request.DeliveryMan.VehicleOwnerTypeId ?? (int?)deliveryMan.Vehicle?.VehicleOwnerType ?? 0;

                var vehicleFrontImagePath = deliveryMan.Vehicle?.FrontImagePath ?? string.Empty;
                var vehicleSideImagePath = deliveryMan.Vehicle?.SideImagePath ?? string.Empty;
                var vehicleFrontLicenseImagePath = deliveryMan.Vehicle?.FrontLicenseImagePath ?? string.Empty;

                vehicleFrontImagePath = await UploadIfBase64Async(request.DeliveryMan.VehicleFrontImagePath, vehicleFrontImagePath) ?? vehicleFrontImagePath;
                vehicleSideImagePath = await UploadIfBase64Async(request.DeliveryMan.VehicleSideImagePath, vehicleSideImagePath) ?? vehicleSideImagePath;
                vehicleFrontLicenseImagePath = await UploadIfBase64Async(request.DeliveryMan.VehicleFrontLicenseImagePath, vehicleFrontLicenseImagePath) ?? vehicleFrontLicenseImagePath;
                var vehicleBackLicenseImagePath = await UploadOrClearDeprecatedAsync(request.DeliveryMan.VehicleBackLicenseImagePath);
                var vehicleFrontInsuranceImagePath = await UploadIfBase64Async(request.DeliveryMan.VehicleFrontInsuranceImagePath, deliveryMan.Vehicle?.FrontInsuranceImagePath);
                var vehicleBackInsuranceImagePath = await UploadOrClearDeprecatedAsync(request.DeliveryMan.VehicleBackInsuranceImagePath);

                var resolvedVehicleLicenseExpiry = vehicleLicenseExpirationDate ?? deliveryMan.Vehicle?.LicenseExpirationDate;
                var resolvedVehicleInsuranceExpiry = vehicleInsuranceExpirationDate ?? deliveryMan.Vehicle?.InSuranceExpirationDate;

                if (deliveryMan.Vehicle != null)
                    _context.DeliveryVehicles.Remove(deliveryMan.Vehicle);

                var addVehicleResult = deliveryMan.AddVehicle(
                    request.DeliveryMan.VehicleTypeId.Value,
                    request.DeliveryMan.VehicleBrandId.Value,
                    request.DeliveryMan.VehiclePlateNumber,
                    vehicleFrontImagePath,
                    vehicleSideImagePath,
                    vehicleFrontLicenseImagePath,
                    vehicleBackLicenseImagePath,
                    resolvedVehicleLicenseExpiry,
                    vehicleFrontInsuranceImagePath,
                    vehicleBackInsuranceImagePath,
                    resolvedVehicleInsuranceExpiry,
                    ownerTypeId
                );

                if (addVehicleResult.IsFailure)
                    return Result.Failure<int>(addVehicleResult.Error);

                if (!string.IsNullOrWhiteSpace(request.DeliveryMan.VehicleOwnerName))
                    await DeliveryManCommandHelper.ApplyVehicleOwnerAsync(deliveryMan, request.DeliveryMan, mediaUploader, deliveryFolder);
            }

            DeliveryManCommandHelper.RefreshCompleteness(deliveryMan);

            var saveResult = await _context.SaveChangesAsyncWithResult();
            if (saveResult.IsFailure)
                return Result.Failure<int>("FailedToSaveData");

            return Result.Success(deliveryMan.Id);
        }
    }
}
