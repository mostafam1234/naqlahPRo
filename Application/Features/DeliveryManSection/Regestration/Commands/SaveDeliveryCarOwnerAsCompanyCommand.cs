using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record SaveDeliveryCarOwnerAsCompanyCommand:IRequest<Result>
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CommercialRecordNumber { get; set; }= string.Empty;
        public string RecordImagePath { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxCertificateImage { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;

        private class SaveDeliveryCarOwnerAsCompanyCommandHandler :
            IRequestHandler<SaveDeliveryCarOwnerAsCompanyCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private readonly IUserSession userSession;
            private const string DeliveryFolderPrefix = "DeliveryMan";
            public SaveDeliveryCarOwnerAsCompanyCommandHandler(INaqlahContext context,
                                                               IMediaUploader mediaUploader,
                                                               IUserSession userSession)
            {
                this.context = context;
                this.mediaUploader = mediaUploader;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveDeliveryCarOwnerAsCompanyCommand request, CancellationToken cancellationToken)
            {
                var userId = userSession.UserId;
                var deliveryMan = await context.DeliveryMen
                                               .Include(x => x.Vehicle)
                                               .AsTracking()
                                               .FirstOrDefaultAsync(x => x.UserId == userId);

                if (deliveryMan is null)
                {
                    return Result.Failure("DeliveryMan Not Found");
                }

                var deliveryFolder = string.Join("{0}_{1}", DeliveryFolderPrefix, deliveryMan.Id);

                var recordImagePath = await mediaUploader.UploadFromBase64(request.RecordImagePath,
                                                                           deliveryFolder);

                var taxCertificateImage = await mediaUploader.UploadFromBase64(request.TaxCertificateImage,
                                                                               deliveryFolder);


                var result = deliveryMan.SetDeliveryVehicleOwnerAsCompany(request.CompanyName,
                                                                     request.CommercialRecordNumber,
                                                                     recordImagePath,
                                                                     request.TaxNumber,
                                                                     taxCertificateImage,
                                                                     request.BankAccountNumber);

                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
