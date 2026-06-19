using Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public AddDeliveryManCommandHandler(
            INaqlahContext context,
            IMediaUploader mediaUploader,
            IUserService userService)
        {
            _context = context;
            this.mediaUploader = mediaUploader;
            this.userService = userService;
        }

        public async Task<Result<int>> Handle(AddDeliveryManCommand request, CancellationToken cancellationToken)
        {
            var requiredValidation = DeliveryManRequiredFieldsValidator.ValidateForAdminCreate(request.DeliveryMan);
            if (requiredValidation.IsFailure)
                return Result.Failure<int>(requiredValidation.Error);

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.BirthDate, out var birthDate) || !birthDate.HasValue)
                return Result.Failure<int>("InvalidBirthDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.IdentityExpirationDate, out var identityExpirationDate))
                return Result.Failure<int>("InvalidIdentityExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.DrivingLicenseExpirationDate, out var licenceExpirationDate))
                return Result.Failure<int>("InvalidDrivingLicenseExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.VehicleLicenseExpirationDate, out var vehicleLicenseExpirationDate))
                return Result.Failure<int>("InvalidVehicleLicenseExpirationDateFormat");

            if (!DeliveryManCommandHelper.TryParseOptionalDate(request.DeliveryMan.VehicleInsuranceExpirationDate, out var vehicleInsuranceExpirationDate))
                return Result.Failure<int>("InvalidVehicleInsuranceExpirationDateFormat");

            var createUserResult = await userService.CreateDeliveryUser(
                request.DeliveryMan.PhoneNumber,
                request.DeliveryMan.Email,
                request.DeliveryMan.FullName,
                request.DeliveryMan.Password
            );

            if (createUserResult.IsFailure)
                return Result.Failure<int>(createUserResult.Error);

            var userId = createUserResult.Value;

            var user = await _context.Users
                .Include(u => u.DeliveryMan)
                .Include(u => u.AspNetUserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null || user.DeliveryMan == null)
                return Result.Failure<int>("FailedToLoadCreatedUser");

            var deliveryManRoleId = Domain.Models.Role.DeliveryMan.Id;
            var hasUserRole = user.AspNetUserRoles.Any(ur => ur.RoleId == deliveryManRoleId);
            if (!hasUserRole)
            {
                var userRole = Domain.Models.UserRole.Instance(deliveryManRoleId);
                userRole.UserId = user.Id;
                user.AspNetUserRoles.Add(userRole);
                var userRoleSaveResult = await _context.SaveChangesAsyncWithResult();
                if (userRoleSaveResult.IsFailure)
                    return Result.Failure<int>("FailedToSaveUserRole");
            }

            var deliveryMan = user.DeliveryMan;
            var deliveryFolder = deliveryMan.Id > 0 
                ? $"{DeliveryFolderPrefix}_{deliveryMan.Id}"
                : $"{DeliveryFolderPrefix}_{userId}";

            var frontIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontIdentityImagePath!, deliveryFolder);
            var frontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.FrontDrivingLicenseImagePath!, deliveryFolder);

            string? backIdenitytImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.BackIdentityImagePath))
                backIdenitytImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackIdentityImagePath, deliveryFolder);

            string? personalImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.PersonalImagePath))
                personalImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.PersonalImagePath, deliveryFolder);

            string? backLicenseImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.BackDrivingLicenseImagePath))
                backLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.BackDrivingLicenseImagePath, deliveryFolder);

            var vehicleFrontImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontImagePath!, deliveryFolder);
            var vehicleSideImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleSideImagePath!, deliveryFolder);
            var vehicleFrontLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontLicenseImagePath!, deliveryFolder);

            string? vehicleBackLicenseImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackLicenseImagePath))
                vehicleBackLicenseImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackLicenseImagePath, deliveryFolder);

            string? vehicleFrontInsuranceImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleFrontInsuranceImagePath))
                vehicleFrontInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleFrontInsuranceImagePath, deliveryFolder);

            string? vehicleBackInsuranceImagePath = null;
            if (!string.IsNullOrEmpty(request.DeliveryMan.VehicleBackInsuranceImagePath))
                vehicleBackInsuranceImagePath = await mediaUploader.UploadFromBase64(request.DeliveryMan.VehicleBackInsuranceImagePath, deliveryFolder);

            var updateResult = deliveryMan.UpdatePersnalInfo(
                request.DeliveryMan.FullName,
                request.DeliveryMan.Address,
                request.DeliveryMan.IdentityNumber,
                frontIdenitytImagePath,
                backIdenitytImagePath,
                personalImagePath,
                identityExpirationDate,
                licenceExpirationDate,
                birthDate,
                request.DeliveryMan.DeliveryType,
                request.DeliveryMan.DeliveryLicenseType,
                frontLicenseImagePath,
                backLicenseImagePath
            );

            if (updateResult.IsFailure)
                return Result.Failure<int>(updateResult.Error);

            deliveryMan.ChangeActivation(request.DeliveryMan.Active);
            deliveryMan.UpdateDeliveryManRequestState((int)DeliveryRequesState.New);

            var addVehicleResult = deliveryMan.AddVehicle(
                request.DeliveryMan.VehicleTypeId!.Value,
                request.DeliveryMan.VehicleBrandId!.Value,
                request.DeliveryMan.VehiclePlateNumber!,
                vehicleFrontImagePath,
                vehicleSideImagePath,
                vehicleFrontLicenseImagePath,
                vehicleBackLicenseImagePath,
                vehicleLicenseExpirationDate,
                vehicleFrontInsuranceImagePath,
                vehicleBackInsuranceImagePath,
                vehicleInsuranceExpirationDate,
                request.DeliveryMan.VehicleOwnerTypeId!.Value
            );

            if (addVehicleResult.IsFailure)
                return Result.Failure<int>(addVehicleResult.Error);

            await DeliveryManCommandHelper.ApplyVehicleOwnerAsync(deliveryMan, request.DeliveryMan, mediaUploader, deliveryFolder);

            DeliveryManCommandHelper.RefreshCompleteness(deliveryMan);

            var saveResult = await _context.SaveChangesAsyncWithResult();
            if (saveResult.IsFailure)
                return Result.Failure<int>("FailedToSaveData");

            return Result.Success(deliveryMan.Id);
        }
    }
}
