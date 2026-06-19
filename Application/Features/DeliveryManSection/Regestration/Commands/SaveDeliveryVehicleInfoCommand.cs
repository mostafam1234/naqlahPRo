using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record SaveDeliveryVehicleInfoCommand:IRequest<Result>
    {
        public int VehicleTypeId { get;  set; }
        public int VehicleBrandId { get;  set; }
        public string? VehicleOwnerName { get; set; }
        public string LicensePlateNumber { get; set; } = string.Empty;
        public string FrontImagePath { get; set; } = string.Empty;
        public string SideImagePath { get; set; } = string.Empty;
        public string FrontLicenseImagePath { get; set; } = string.Empty;
        public string? BackLicenseImagePath { get; set; }
        public string? LicenseExpirationDate { get; set; }
        public string? FrontInsuranceImagePath { get; set; }
        public string? BackInsuranceImagePath { get; set; }
        public string? InSuranceExpirationDate { get; set; }
        public int VehicleOwnerTypeId { get;  set; }

        private class SaveDeliveryVehicleInfoCommandHandler :
            IRequestHandler<SaveDeliveryVehicleInfoCommand, Result>
        {
            private readonly IUserSession userSession;
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private const string DeliveryFolderPrefix = "DeliveryMan";
            public SaveDeliveryVehicleInfoCommandHandler(IUserSession userSession,
                                                         INaqlahContext context,
                                                         IMediaUploader mediaUploader)
            {
                this.userSession = userSession;
                this.context = context;
                this.mediaUploader = mediaUploader;
            }
            public async Task<Result> Handle(SaveDeliveryVehicleInfoCommand request, CancellationToken cancellationToken)
            {
                var userId = userSession.UserId;
                var deliveryMan = await context.DeliveryMen
                                             .AsTracking()
                                             .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

                if (deliveryMan is null)
                    return Result.Failure("DeliveryMan Not Found");

                var deliveryFolder = $"{DeliveryFolderPrefix}_{deliveryMan.Id}";

                var frontImage = await mediaUploader.UploadFromBase64(request.FrontImagePath, deliveryFolder);
                var sideImage = await mediaUploader.UploadFromBase64(request.SideImagePath, deliveryFolder);
                var frontLicense = await mediaUploader.UploadFromBase64(request.FrontLicenseImagePath, deliveryFolder);

                string? backLicense = null;
                if (!string.IsNullOrWhiteSpace(request.BackLicenseImagePath))
                    backLicense = await mediaUploader.UploadFromBase64(request.BackLicenseImagePath, deliveryFolder);

                string? frontInsurance = null;
                if (!string.IsNullOrWhiteSpace(request.FrontInsuranceImagePath))
                    frontInsurance = await mediaUploader.UploadFromBase64(request.FrontInsuranceImagePath, deliveryFolder);

                string? backInsurance = null;
                if (!string.IsNullOrWhiteSpace(request.BackInsuranceImagePath))
                    backInsurance = await mediaUploader.UploadFromBase64(request.BackInsuranceImagePath, deliveryFolder);

                DateTime? licenseExpirationDate = null;
                if (!string.IsNullOrWhiteSpace(request.LicenseExpirationDate))
                {
                    if (!DateTime.TryParseExact(request.LicenseExpirationDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        return Result.Failure("Invalid license expiration date");
                    licenseExpirationDate = parsed;
                }

                DateTime? inSuranceExpirationDate = null;
                if (!string.IsNullOrWhiteSpace(request.InSuranceExpirationDate))
                {
                    if (!DateTime.TryParseExact(request.InSuranceExpirationDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        return Result.Failure("Invalid insurance expiration date");
                    inSuranceExpirationDate = parsed;
                }

                var vehicleResult = deliveryMan.AddVehicle(request.VehicleTypeId,
                                                          request.VehicleBrandId,
                                                          request.LicensePlateNumber,
                                                          frontImage,
                                                          sideImage,
                                                          frontLicense,
                                                          backLicense,
                                                          licenseExpirationDate,
                                                          frontInsurance,
                                                          backInsurance,
                                                          inSuranceExpirationDate,
                                                          request.VehicleOwnerTypeId,
                                                          request.VehicleOwnerName);

                if (vehicleResult.IsFailure)
                    return vehicleResult;

                DeliveryManCommandHelper.RefreshCompleteness(deliveryMan);
                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
