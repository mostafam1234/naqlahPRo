using CSharpFunctionalExtensions;
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
    public sealed record SaveDeliveryManInfoCommand:IRequest<Result>
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FrontIdenitytImage { get; set; } = string.Empty;
        public string BackIdenitytImage { get; set; } = string.Empty;
        public string PersonalImage { get; set; } = string.Empty;
        public string IdentityExpirationDate { get; set; } = string.Empty;
        public string DrivingLicenseExpirationDate { get; set; } = string.Empty;
        public int DeliveryTypeId { get; set; }
        public int DeliveryLicenseTypeId { get; set; }
        public string FrontDrivingLicenseImage { get; set; } = string.Empty;
        public string BackDrivingLicenseImage { get; set; } = string.Empty;

        private class SaveDeliveryManInfoCommandHandler : IRequestHandler<SaveDeliveryManInfoCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private readonly IUserSession userSession;
            private const string DeliveryFolderPrefix = "DeliveryMan";
            public SaveDeliveryManInfoCommandHandler(INaqlahContext context,
                                                     IMediaUploader mediaUploader,
                                                     IUserSession userSession)
            {
                this.context = context;
                this.mediaUploader = mediaUploader;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveDeliveryManInfoCommand request, CancellationToken cancellationToken)
            {
                var userId = userSession.UserId;
                var deliveryMan = await context.DeliveryMen
                                             .AsTracking()
                                             .Where(x => x.UserId == userId)
                                             .FirstOrDefaultAsync();

                if (deliveryMan == null)
                {
                    return Result.Failure("Delivery Man Not Found");
                }

                DateTime identityExpirationDate;
                DateTime licenceExpirationDate;
                DateTime.TryParseExact(request.IdentityExpirationDate, "yyyy/MM/dd", new CultureInfo("en-US"), DateTimeStyles.None, out identityExpirationDate);
                DateTime.TryParseExact(request.DrivingLicenseExpirationDate, "yyyy/MM/dd", new CultureInfo("en-US"), DateTimeStyles.None, out licenceExpirationDate);

                var deliveryFolder = string.Join("{0}_1", DeliveryFolderPrefix, deliveryMan.Id);

                var frontIdenitytImagePath = await mediaUploader.UploadFromBase64(request.FrontIdenitytImage,
                                                                                deliveryFolder);
                var backIdenitytImagePath = await mediaUploader.UploadFromBase64(request.BackIdenitytImage,
                                                                                 deliveryFolder);

                var personalImage = await mediaUploader.UploadFromBase64(request.PersonalImage,
                                                                        deliveryFolder);
                 
                var frontLicenseImage= await mediaUploader.UploadFromBase64(request.FrontDrivingLicenseImage,
                                                                            deliveryFolder);

                var backLicenseImage= await mediaUploader.UploadFromBase64(request.BackDrivingLicenseImage,
                                                                                 deliveryFolder);


                var updateResult = deliveryMan.UpdatePersnalInfo(request.FullName,
                                                               request.Address,
                                                               request.IdentityNumber,
                                                               frontIdenitytImagePath,
                                                               backIdenitytImagePath,
                                                               personalImage,
                                                               identityExpirationDate,
                                                               licenceExpirationDate,
                                                               request.DeliveryTypeId,
                                                               request.DeliveryLicenseTypeId,
                                                               frontLicenseImage,
                                                               backLicenseImage);

                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;

            }
        }

    }
}
