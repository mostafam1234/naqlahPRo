using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record SaveDeliveryManInfoCommand:IRequest<Result>
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string? BirthDate { get; set; }
        public string FrontIdenitytImage { get; set; } = string.Empty;
        public string? BackIdenitytImage { get; set; }
        public string? PersonalImage { get; set; }
        public string? IdentityExpirationDate { get; set; }
        public string? DrivingLicenseExpirationDate { get; set; }
        public int DeliveryTypeId { get; set; }
        public int DeliveryLicenseTypeId { get; set; }
        public string FrontDrivingLicenseImage { get; set; } = string.Empty;
        public string? BackDrivingLicenseImage { get; set; }

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
                                             .Include(x => x.Vehicle!)
                                                .ThenInclude(v => v.Resident)
                                             .Include(x => x.Vehicle!)
                                                .ThenInclude(v => v.Company)
                                             .Include(x => x.Vehicle!)
                                                .ThenInclude(v => v.Renter)
                                             .AsTracking()
                                             .Where(x => x.UserId == userId)
                                             .FirstOrDefaultAsync(cancellationToken);

                if (deliveryMan == null)
                    return Result.Failure("Delivery Man Not Found");

                DateTime? identityExpirationDate = null;
                DateTime? licenceExpirationDate = null;
                DateTime? birthDate = null;

                if (!string.IsNullOrWhiteSpace(request.IdentityExpirationDate))
                {
                    if (!DateTime.TryParseExact(request.IdentityExpirationDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        return Result.Failure("Invalid identity expiration date");
                    identityExpirationDate = parsed;
                }

                if (!string.IsNullOrWhiteSpace(request.DrivingLicenseExpirationDate))
                {
                    if (!DateTime.TryParseExact(request.DrivingLicenseExpirationDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        return Result.Failure("Invalid driving license expiration date");
                    licenceExpirationDate = parsed;
                }

                if (!string.IsNullOrWhiteSpace(request.BirthDate))
                {
                    if (!DateTime.TryParseExact(request.BirthDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                        && !DateTime.TryParseExact(request.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                        return Result.Failure("Invalid birth date");
                    birthDate = parsed;
                }

                var deliveryFolder = $"{DeliveryFolderPrefix}_{deliveryMan.Id}";

                var frontIdenitytImagePath = await mediaUploader.UploadFromBase64(request.FrontIdenitytImage, deliveryFolder);
                var frontLicenseImage = await mediaUploader.UploadFromBase64(request.FrontDrivingLicenseImage, deliveryFolder);

                string? backIdenitytImagePath = null;
                if (!string.IsNullOrWhiteSpace(request.BackIdenitytImage))
                    backIdenitytImagePath = await mediaUploader.UploadFromBase64(request.BackIdenitytImage, deliveryFolder);

                string? personalImage = null;
                if (!string.IsNullOrWhiteSpace(request.PersonalImage))
                    personalImage = await mediaUploader.UploadFromBase64(request.PersonalImage, deliveryFolder);

                string? backLicenseImage = null;
                if (!string.IsNullOrWhiteSpace(request.BackDrivingLicenseImage))
                    backLicenseImage = await mediaUploader.UploadFromBase64(request.BackDrivingLicenseImage, deliveryFolder);

                var updateResult = deliveryMan.UpdatePersnalInfo(request.FullName,
                                               request.Address,
                                               request.IdentityNumber,
                                               frontIdenitytImagePath,
                                               backIdenitytImagePath,
                                               personalImage,
                                               identityExpirationDate,
                                               licenceExpirationDate,
                                               birthDate,
                                               request.DeliveryTypeId,
                                               request.DeliveryLicenseTypeId,
                                               frontLicenseImage,
                                               backLicenseImage);

                if (updateResult.IsFailure)
                    return Result.Failure(updateResult.Error);

                DeliveryManCommandHelper.RefreshCompleteness(deliveryMan);
                return await context.SaveChangesAsyncWithResult();
            }
        }
    }
}
