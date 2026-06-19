using Application.Shared.Services;
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
    public sealed record SaveDeliveryCarOwnerAsRenterCommand:IRequest<Result>
    {
        public string CitizenName { get; set; }=string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FrontIdentityImage { get; set; } = string.Empty;
        public string? BackIdentityImage { get; set; }
        public string? RentContractImage { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;


        private class SaveDeliveryCarOwnerAsRenterCommandHandler :
            IRequestHandler<SaveDeliveryCarOwnerAsRenterCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private readonly IUserSession userSession;
            private const string DeliveryFolderPrefix = "DeliveryMan";
            public SaveDeliveryCarOwnerAsRenterCommandHandler(INaqlahContext context,
                                                              IMediaUploader mediaUploader,
                                                              IUserSession userSession)
            {
                this.context = context;
                this.mediaUploader = mediaUploader;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveDeliveryCarOwnerAsRenterCommand request, CancellationToken cancellationToken)
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

                var frontImagePath = await mediaUploader.UploadFromBase64(request.FrontIdentityImage,
                                                                           deliveryFolder);

                string? backImagePath = null;
                if (!string.IsNullOrWhiteSpace(request.BackIdentityImage))
                    backImagePath = await mediaUploader.UploadFromBase64(request.BackIdentityImage, deliveryFolder);

                string? rentContractImagePath = null;
                if (!string.IsNullOrWhiteSpace(request.RentContractImage))
                    rentContractImagePath = await mediaUploader.UploadFromBase64(request.RentContractImage, deliveryFolder);

               var result= deliveryMan.SetDeliveryVehicleOwnerAsRenter(request.CitizenName,
                                                                       request.IdentityNumber,
                                                                       frontImagePath,
                                                                       backImagePath,
                                                                       rentContractImagePath,
                                                                       request.BankAccountNumber);

                if (result.IsFailure)
                {
                    return result;
                }

                DeliveryManCommandHelper.RefreshCompleteness(deliveryMan);
                return await context.SaveChangesAsyncWithResult();

            }
        }
    }
}
