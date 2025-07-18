using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record SaveDeliveryVehicleInfoCommand:IRequest<Result>
    {
        public int VehicleTypeId { get;  set; }
        public int VehicleBrandId { get;  set; }
        public string LicensePlateNumber { get; set; } = string.Empty;
        public string FrontImagePath { get; set; } = string.Empty;
        public string SideImagePath { get; set; } = string.Empty;
        public string FrontLicenseImagePath { get; set; } = string.Empty;
        public string BackLicenseImagePath { get; set; } = string.Empty;
        public string LicenseExpirationDate { get; set; } = string.Empty;
        public string FrontInsuranceImagePath { get; set; } = string.Empty;
        public string BackInsuranceImagePath { get; set; } = string.Empty;
        public string InSuranceExpirationDate { get; set; } = string.Empty;
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
                                             .FirstOrDefaultAsync(x => x.UserId == userId);

                if (deliveryMan is null)
                {
                    return Result.Failure("DeliveryMan Not Found");
                }

                var deliveryFolder = string.Join("{0}_{1}", DeliveryFolderPrefix, deliveryMan.Id);

                var frontImage = await mediaUploader.UploadFromBase64(request.FrontImagePath,
                                                                      deliveryFolder);

                var sideImage = await mediaUploader.UploadFromBase64(request.SideImagePath,
                                                                      deliveryFolder);
                var frontLicense = await mediaUploader.UploadFromBase64(request.FrontLicenseImagePath,
                                                                      deliveryFolder);
                var backLicense = await mediaUploader.UploadFromBase64(request.BackInsuranceImagePath,
                                                                      deliveryFolder);
                var frontInsurance = await mediaUploader
                    .UploadFromBase64(request.FrontInsuranceImagePath,
                                      deliveryFolder);

                var backInsurance = await mediaUploader.UploadFromBase64(request.BackInsuranceImagePath,
                                                                         deliveryFolder);

                DateTime licenseExpirationDate;
                DateTime inSuranceExpirationDate;
                DateTime.TryParseExact(request.LicenseExpirationDate, "yyyy/MM/dd", new CultureInfo("en-US"), DateTimeStyles.None, out licenseExpirationDate);
                DateTime.TryParseExact(request.InSuranceExpirationDate, "yyyy/MM/dd", new CultureInfo("en-US"), DateTimeStyles.None, out inSuranceExpirationDate);


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
                                                          request.VehicleOwnerTypeId
                                                         );
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
