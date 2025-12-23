using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands
{
    public class AddDeliveryManCommandHandler : IRequestHandler<AddDeliveryManCommand, Result<int>>   
    {
        private readonly INaqlahContext _context;
        private readonly IMediaUploader mediaUploader;
        private readonly IUserService userService;
        private const string DeliveryFolderPrefix = "DeliveryMan";

        public AddDeliveryManCommandHandler(INaqlahContext context, IMediaUploader mediaUploader, IUserService userService)
        {
            _context = context;
            this.mediaUploader = mediaUploader;
            this.userService = userService;
        }

        public async Task<Result<int>> Handle(AddDeliveryManCommand request, CancellationToken cancellationToken)
        {
            // Validate required user fields
            if (string.IsNullOrWhiteSpace(request.DeliveryMan.Email))
            {
                return Result.Failure<int>("Email is required");
            }

            if (string.IsNullOrWhiteSpace(request.DeliveryMan.Password))
            {
                return Result.Failure<int>("Password is required");
            }

            // Step 1: Create user account first (this also creates a basic DeliveryMan)
            var createUserResult = await userService.CreateDeliveryUser(
                request.DeliveryMan.PhoneNumber,
                request.DeliveryMan.Email,
                request.DeliveryMan.FullName,
                request.DeliveryMan.Password
            );

            if (createUserResult.IsFailure)
            {
                return Result.Failure<int>(createUserResult.Error);
            }

            var userId = createUserResult.Value;

            // Step 2: Load the user with delivery man from context
            var user = await _context.Users
                .Include(u => u.DeliveryMan)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null || user.DeliveryMan == null)
            {
                return Result.Failure<int>("Failed to load created user or delivery man");
            }

            var deliveryMan = user.DeliveryMan;
            // Note: deliveryMan is already tracked by EF Core since it was loaded with Include

            // Step 3: Parse dates
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

            // Step 4: Create delivery folder using delivery man ID (use userId as temporary if Id is 0)
            var deliveryFolder = deliveryMan.Id > 0 
                ? string.Format("{0}_{1}", DeliveryFolderPrefix, deliveryMan.Id)
                : string.Format("{0}_{1}", DeliveryFolderPrefix, userId);

            var frontIdenitytImagePath = "";  var backIdenitytImagePath = ""; var personalImagePath = "";
            var frontLicenseImagePath = ""; var backLicenseImagePath = "";
            
            // Vehicle image paths
            var vehicleFrontImagePath = ""; var vehicleSideImagePath = "";
            var vehicleFrontLicenseImagePath = ""; var vehicleBackLicenseImagePath = "";
            var vehicleFrontInsuranceImagePath = ""; var vehicleBackInsuranceImagePath = "";
            
            // Upload delivery man images
            if (!string.IsNullOrEmpty(request.DeliveryMan.FrontIdentityImagePath))
            {
                frontIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontIdentityImagePath, deliveryFolder);
            }
            if(!string.IsNullOrEmpty(request.DeliveryMan.BackIdentityImagePath))
            {
                backIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackIdentityImagePath, deliveryFolder);
            }

            if(!string.IsNullOrEmpty(request.DeliveryMan.PersonalImagePath))
            {
                personalImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.PersonalImagePath, deliveryFolder);
            }

            if(!string.IsNullOrEmpty(request.DeliveryMan.FrontDrivingLicenseImagePath))
            {
                frontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontDrivingLicenseImagePath, deliveryFolder);
            }

            if (!string.IsNullOrEmpty(request.DeliveryMan.BackDrivingLicenseImagePath))
            {
                backLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackDrivingLicenseImagePath, deliveryFolder);
            }
            
            // Upload vehicle images
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontImagePath))
            {
                vehicleFrontImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontImagePath, deliveryFolder);
            }
            
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleSideImagePath))
            {
                vehicleSideImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleSideImagePath, deliveryFolder);
            }
            
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontLicenseImagePath))
            {
                vehicleFrontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontLicenseImagePath, deliveryFolder);
            }
            
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackLicenseImagePath))
            {
                vehicleBackLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackLicenseImagePath, deliveryFolder);
            }
            
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontInsuranceImagePath))
            {
                vehicleFrontInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontInsuranceImagePath, deliveryFolder);
            }
            
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackInsuranceImagePath))
            {
                vehicleBackInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackInsuranceImagePath, deliveryFolder);
            }


            // Step 5: Update delivery man personal information using domain method
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

            // Step 6: Set activation status
            deliveryMan.ChangeActivation(request.DeliveryMan.Active);

            // Step 7: Set delivery state (default to New for new delivery men)
            deliveryMan.UpdateDeliveryManRequestState((int)DeliveryRequesState.New);

            // Step 8: Add vehicle using domain method (this sets the relationship properly)
            if (request.DeliveryMan.VehicleTypeId.HasValue && 
                request.DeliveryMan.VehicleBrandId.HasValue && 
                !string.IsNullOrWhiteSpace(request.DeliveryMan.VehiclePlateNumber))
            {
                var ownerTypeId = request.DeliveryMan.VehicleOwnerTypeId ?? 0;

                // Use default dates if not provided
                if (vehicleLicenseExpirationDate == DateTime.MinValue)
                {
                    vehicleLicenseExpirationDate = DateTime.Now.AddYears(1);
                }

                if (vehicleInsuranceExpirationDate == DateTime.MinValue)
                {
                    vehicleInsuranceExpirationDate = DateTime.Now.AddYears(1);
                }

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

            // Step 9: Save all changes in a single transaction
            // EF Core will automatically handle all relationships and save:
            // - DeliveryMan updates (address, IdentityNumber, images, DeliveryType, DeliveryLicenseType, Active, DeliveryState)
            // - DeliveryVehicle (through the navigation property relationship)
            var saveResult = await _context.SaveChangesAsyncWithResult();

            if (saveResult.IsFailure)
                return Result.Failure<int>($"Failed To Save Data: {saveResult.Error}");

            return Result.Success(deliveryMan.Id);
        }
    }
}