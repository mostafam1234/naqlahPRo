using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
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

        public UpdateDeliveryManCommandHandler(INaqlahContext context, IMediaUploader mediaUploader, UserManager<User> userManager)
        {
            _context = context;
            this.mediaUploader = mediaUploader;
            this._userManager = userManager;
        }

        public async Task<Result<int>> Handle(UpdateDeliveryManCommand request, CancellationToken cancellationToken)
        {
            // Load existing delivery man with related data
            var deliveryMan = await _context.DeliveryMen
                .Include(x => x.User)
                .Include(x => x.Vehicle)
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == request.DeliveryManId, cancellationToken);

            if (deliveryMan == null)
            {
                return Result.Failure<int>("Delivery man not found");
            }

            // Update user email if provided and different
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

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.DeliveryMan.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.DeliveryMan.Password);
                    if (!passwordResult.Succeeded)
                    {
                        return Result.Failure<int>($"Failed to update password: {string.Join(", ", passwordResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Parse dates
            DateTime identityExpirationDate;
            DateTime licenceExpirationDate;
            DateTime vehicleLicenseExpirationDate = DateTime.MinValue;
            DateTime vehicleInsuranceExpirationDate = DateTime.MinValue;

            if (!DateTime.TryParseExact(request.DeliveryMan.IdentityExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out identityExpirationDate))
            {
                return Result.Failure<int>("Invalid Identity Expiration Date format");
            }

            if (!DateTime.TryParseExact(request.DeliveryMan.DrivingLicenseExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out licenceExpirationDate))
            {
                return Result.Failure<int>("Invalid Driving License Expiration Date format");
            }

            if (!string.IsNullOrWhiteSpace(request.DeliveryMan.VehicleLicenseExpirationDate))
            {
                if (!DateTime.TryParseExact(request.DeliveryMan.VehicleLicenseExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out vehicleLicenseExpirationDate))
                {
                    return Result.Failure<int>("Invalid Vehicle License Expiration Date format");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.DeliveryMan.VehicleInsuranceExpirationDate))
            {
                if (!DateTime.TryParseExact(request.DeliveryMan.VehicleInsuranceExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out vehicleInsuranceExpirationDate))
                {
                    return Result.Failure<int>("Invalid Vehicle Insurance Expiration Date format");
                }
            }

            // Create delivery folder
            var deliveryFolder = string.Format("{0}_{1}", DeliveryFolderPrefix, deliveryMan.Id);

            // Helper to check if string is base64 (new image) or URL (existing image)
            bool IsBase64Image(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                return value.StartsWith("data:image/") || (value.Length > 100 && !value.StartsWith("http"));
            }

            // Handle image uploads - only upload if it's a new base64 image
            var frontIdenitytImagePath = deliveryMan.FrontIdenitytImagePath ?? "";
            var backIdenitytImagePath = deliveryMan.BackIdenitytImagePath ?? "";
            var personalImagePath = deliveryMan.PersonalImagePath ?? "";
            var frontLicenseImagePath = deliveryMan.FrontDrivingLicenseImagePath ?? "";
            var backLicenseImagePath = deliveryMan.BackDrivingLicenseImagePath ?? "";

            if (!string.IsNullOrEmpty(request.DeliveryMan.FrontIdentityImagePath) && IsBase64Image(request.DeliveryMan.FrontIdentityImagePath))
            {
                frontIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontIdentityImagePath, deliveryFolder);
            }
            else if (!string.IsNullOrEmpty(request.DeliveryMan.FrontIdentityImagePath))
            {
                // Keep existing path if URL provided
                frontIdenitytImagePath = request.DeliveryMan.FrontIdentityImagePath.Contains("/ImageBank/") 
                    ? request.DeliveryMan.FrontIdentityImagePath.Split('/').Last() 
                    : deliveryMan.FrontIdenitytImagePath ?? "";
            }

            if (!string.IsNullOrEmpty(request.DeliveryMan.BackIdentityImagePath) && IsBase64Image(request.DeliveryMan.BackIdentityImagePath))
            {
                backIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackIdentityImagePath, deliveryFolder);
            }
            else if (!string.IsNullOrEmpty(request.DeliveryMan.BackIdentityImagePath))
            {
                backIdenitytImagePath = request.DeliveryMan.BackIdentityImagePath.Contains("/ImageBank/") 
                    ? request.DeliveryMan.BackIdentityImagePath.Split('/').Last() 
                    : deliveryMan.BackIdenitytImagePath ?? "";
            }

            if (!string.IsNullOrEmpty(request.DeliveryMan.PersonalImagePath) && IsBase64Image(request.DeliveryMan.PersonalImagePath))
            {
                personalImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.PersonalImagePath, deliveryFolder);
            }
            else if (!string.IsNullOrEmpty(request.DeliveryMan.PersonalImagePath))
            {
                personalImagePath = request.DeliveryMan.PersonalImagePath.Contains("/ImageBank/") 
                    ? request.DeliveryMan.PersonalImagePath.Split('/').Last() 
                    : deliveryMan.PersonalImagePath ?? "";
            }

            if (!string.IsNullOrEmpty(request.DeliveryMan.FrontDrivingLicenseImagePath) && IsBase64Image(request.DeliveryMan.FrontDrivingLicenseImagePath))
            {
                frontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontDrivingLicenseImagePath, deliveryFolder);
            }
            else if (!string.IsNullOrEmpty(request.DeliveryMan.FrontDrivingLicenseImagePath))
            {
                frontLicenseImagePath = request.DeliveryMan.FrontDrivingLicenseImagePath.Contains("/ImageBank/") 
                    ? request.DeliveryMan.FrontDrivingLicenseImagePath.Split('/').Last() 
                    : deliveryMan.FrontDrivingLicenseImagePath ?? "";
            }

            if (!string.IsNullOrEmpty(request.DeliveryMan.BackDrivingLicenseImagePath) && IsBase64Image(request.DeliveryMan.BackDrivingLicenseImagePath))
            {
                backLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackDrivingLicenseImagePath, deliveryFolder);
            }
            else if (!string.IsNullOrEmpty(request.DeliveryMan.BackDrivingLicenseImagePath))
            {
                backLicenseImagePath = request.DeliveryMan.BackDrivingLicenseImagePath.Contains("/ImageBank/") 
                    ? request.DeliveryMan.BackDrivingLicenseImagePath.Split('/').Last() 
                    : deliveryMan.BackDrivingLicenseImagePath ?? "";
            }

            // Update delivery man personal information
            var updateResult = deliveryMan.UpdatePersnalInfo(
                request.DeliveryMan.FullName,
                request.DeliveryMan.Address,
                request.DeliveryMan.IdentityNumber,
                frontIdenitytImagePath,
                backIdenitytImagePath,
                personalImagePath,
                identityExpirationDate,
                licenceExpirationDate,
                request.DeliveryMan.DeliveryType,
                request.DeliveryMan.DeliveryLicenseType,
                frontLicenseImagePath,
                backLicenseImagePath
            );

            if (updateResult.IsFailure)
                return Result.Failure<int>(updateResult.Error);


            // Handle vehicle update or creation
            if (request.DeliveryMan.VehicleTypeId.HasValue &&
                request.DeliveryMan.VehicleBrandId.HasValue &&
                !string.IsNullOrWhiteSpace(request.DeliveryMan.VehiclePlateNumber))
            {
                var ownerTypeId = request.DeliveryMan.VehicleOwnerTypeId ?? 0;

                // Use provided dates or default
                if (vehicleLicenseExpirationDate == DateTime.MinValue)
                {
                    vehicleLicenseExpirationDate = DateTime.Now.AddYears(1);
                }

                if (vehicleInsuranceExpirationDate == DateTime.MinValue)
                {
                    vehicleInsuranceExpirationDate = DateTime.Now.AddYears(1);
                }

                // Handle vehicle images
                var vehicleFrontImagePath = "";
                var vehicleSideImagePath = "";
                var vehicleFrontLicenseImagePath = "";
                var vehicleBackLicenseImagePath = "";
                var vehicleFrontInsuranceImagePath = "";
                var vehicleBackInsuranceImagePath = "";

                if (deliveryMan.Vehicle != null)
                {
                    // Keep existing paths as defaults
                    vehicleFrontImagePath = deliveryMan.Vehicle.FrontImagePath ?? "";
                    vehicleSideImagePath = deliveryMan.Vehicle.SideImagePath ?? "";
                    vehicleFrontLicenseImagePath = deliveryMan.Vehicle.FrontLicenseImagePath ?? "";
                    vehicleBackLicenseImagePath = deliveryMan.Vehicle.BackLicenseImagePath ?? "";
                    vehicleFrontInsuranceImagePath = deliveryMan.Vehicle.FrontInsuranceImagePath ?? "";
                    vehicleBackInsuranceImagePath = deliveryMan.Vehicle.BackInsuranceImagePath ?? "";
                }

                // Upload new vehicle images if provided as base64
                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontImagePath) && IsBase64Image(request.DeliveryMan.VehicleFrontImagePath))
                {
                    vehicleFrontImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontImagePath, deliveryFolder);
                }

                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleSideImagePath) && IsBase64Image(request.DeliveryMan.VehicleSideImagePath))
                {
                    vehicleSideImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleSideImagePath, deliveryFolder);
                }

                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontLicenseImagePath) && IsBase64Image(request.DeliveryMan.VehicleFrontLicenseImagePath))
                {
                    vehicleFrontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontLicenseImagePath, deliveryFolder);
                }

                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackLicenseImagePath) && IsBase64Image(request.DeliveryMan.VehicleBackLicenseImagePath))
                {
                    vehicleBackLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackLicenseImagePath, deliveryFolder);
                }

                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontInsuranceImagePath) && IsBase64Image(request.DeliveryMan.VehicleFrontInsuranceImagePath))
                {
                    vehicleFrontInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontInsuranceImagePath, deliveryFolder);
                }

                if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackInsuranceImagePath) && IsBase64Image(request.DeliveryMan.VehicleBackInsuranceImagePath))
                {
                    vehicleBackInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackInsuranceImagePath, deliveryFolder);
                }

                if (deliveryMan.Vehicle != null)
                {
                    // Update existing vehicle - remove and add new one since there's no comprehensive update method
                    var oldVehicleId = deliveryMan.Vehicle.Id;
                    _context.DeliveryVehicles.Remove(deliveryMan.Vehicle);
                    
                    // Add new vehicle with updated data
                    var addVehicleResult = deliveryMan.AddVehicle(
                        request.DeliveryMan.VehicleTypeId.Value,
                        request.DeliveryMan.VehicleBrandId.Value,
                        request.DeliveryMan.VehiclePlateNumber,
                        vehicleFrontImagePath,
                        vehicleSideImagePath,
                        vehicleFrontLicenseImagePath,
                        vehicleBackLicenseImagePath,
                        vehicleLicenseExpirationDate,
                        vehicleFrontInsuranceImagePath,
                        vehicleBackInsuranceImagePath,
                        vehicleInsuranceExpirationDate,
                        ownerTypeId
                    );

                    if (addVehicleResult.IsFailure)
                    {
                        return Result.Failure<int>(addVehicleResult.Error);
                    }
                }
                else
                {
                    // Add new vehicle
                    var addVehicleResult = deliveryMan.AddVehicle(
                        request.DeliveryMan.VehicleTypeId.Value,
                        request.DeliveryMan.VehicleBrandId.Value,
                        request.DeliveryMan.VehiclePlateNumber,
                        vehicleFrontImagePath,
                        vehicleSideImagePath,
                        vehicleFrontLicenseImagePath,
                        vehicleBackLicenseImagePath,
                        vehicleLicenseExpirationDate,
                        vehicleFrontInsuranceImagePath,
                        vehicleBackInsuranceImagePath,
                        vehicleInsuranceExpirationDate,
                        ownerTypeId
                    );

                    if (addVehicleResult.IsFailure)
                    {
                        return Result.Failure<int>(addVehicleResult.Error);
                    }
                }
            }

            // Save all changes
            var saveResult = await _context.SaveChangesAsyncWithResult();

            if (saveResult.IsFailure)
                return Result.Failure<int>($"Failed To Save Data: {saveResult.Error}");

            return Result.Success(deliveryMan.Id);
        }
    }
}

