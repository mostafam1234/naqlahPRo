using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands
{
    public class AddDeliveryManCommandHandler : IRequestHandler<AddDeliveryManCommand, Result<int>>   
    {
        private readonly INaqlahContext _context;
        private readonly IMediaUploader mediaUploader;
        private const string DeliveryFolderPrefix = "DeliveryMan";

        public AddDeliveryManCommandHandler(INaqlahContext context, IMediaUploader mediaUploader)
        {
            _context = context;
            this.mediaUploader = mediaUploader;
        }

        public async Task<Result<int>> Handle(AddDeliveryManCommand request, CancellationToken cancellationToken)
        {
            var deliveryMan = new DeliveryMan();
            DateTime identityExpirationDate;
            DateTime licenceExpirationDate;
            DateTime vehicleLicenseExpirationDate;
            DateTime vehicleInsuranceExpirationDate;
            
            DateTime.TryParseExact(request.DeliveryMan.IdentityExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out identityExpirationDate);
            DateTime.TryParseExact(request.DeliveryMan.DrivingLicenseExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out licenceExpirationDate);
            DateTime.TryParseExact(request.DeliveryMan.VehicleLicenseExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out vehicleLicenseExpirationDate);
            DateTime.TryParseExact(request.DeliveryMan.VehicleInsuranceExpirationDate, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out vehicleInsuranceExpirationDate);

            var deliveryFolder = string.Join("{0}_{1}", DeliveryFolderPrefix, deliveryMan.Id);

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


            var updateResult = deliveryMan.UpdatePersnalInfo(request.DeliveryMan.FullName,
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
                                                           backLicenseImagePath);

            if (updateResult.IsFailure)
                return Result.Failure<int>(updateResult.Error);

            // Set activation status
            deliveryMan.ChangeActivation(request.DeliveryMan.Active);

            await _context.DeliveryMen.AddAsync(deliveryMan, cancellationToken);
            var saveResult = await _context.SaveChangesAsyncWithResult();

            if (saveResult.IsFailure)
                return Result.Failure<int>("Failed To Save Data");

            // Create delivery vehicle if vehicle data is provided
            if (request.DeliveryMan.VehicleTypeId.HasValue && request.DeliveryMan.VehicleBrandId.HasValue && !string.IsNullOrWhiteSpace(request.DeliveryMan.VehiclePlateNumber))
            {
                var ownerTypeId = request.DeliveryMan.VehicleOwnerTypeId ?? 0;

                var vehicleResult = DeliveryVehicle.Instance(
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

                if (vehicleResult.IsSuccess)
                {
                    // Set the DeliveryManId for the vehicle
                    var vehicle = vehicleResult.Value;
                    typeof(DeliveryVehicle).GetProperty("DeliveryManId")?.SetValue(vehicle, deliveryMan.Id);

                    await _context.DeliveryVehicles.AddAsync(vehicle, cancellationToken);
                    var vehicleSaveResult = await _context.SaveChangesAsyncWithResult();

                    if (vehicleSaveResult.IsFailure)
                        return Result.Failure<int>("Failed To Save Vehicle Data");
                }
            }

            return Result.Success(deliveryMan.Id);
        }
    }
}