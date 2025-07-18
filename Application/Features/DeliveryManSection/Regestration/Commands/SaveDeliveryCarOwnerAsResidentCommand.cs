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
    public sealed record SaveDeliveryCarOwnerAsResidentCommand:IRequest<Result>
    {
        public string CitizenName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FrontIdentityImage { get; set; } = string.Empty;
        public string BackIdentityImage { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;

        private class SaveDeliveryCarOwnerAsResidentCommandHandler :
            IRequestHandler<SaveDeliveryCarOwnerAsResidentCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private readonly IUserSession userSession;
            private const string DeliveryFolderPrefix = "DeliveryMan";

            public SaveDeliveryCarOwnerAsResidentCommandHandler(INaqlahContext context,
                                                                IMediaUploader mediaUploader,
                                                                IUserSession userSession)
            {
                this.context = context;
                this.mediaUploader = mediaUploader;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveDeliveryCarOwnerAsResidentCommand request, CancellationToken cancellationToken)
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

                var deliveryFolder = string.Join("{0}_1", DeliveryFolderPrefix, deliveryMan.Id);

                var frontImage = await mediaUploader.UploadFromBase64(request.FrontIdentityImage,
                                                                      deliveryFolder);

                var backImage = await mediaUploader.UploadFromBase64(request.BackIdentityImage,
                                                                      deliveryFolder);

                var carOwnerResult = deliveryMan.SetDeliveryVehicleOwnerAsResident(request.CitizenName,
                                                                                 request.IdentityNumber,
                                                                                 frontImage,
                                                                                 backImage,
                                                                                 request.BankAccountNumber);

                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;



            }
        }
    }
}
