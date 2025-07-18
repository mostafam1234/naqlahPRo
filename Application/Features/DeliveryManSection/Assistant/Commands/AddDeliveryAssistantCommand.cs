using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Assistant.Commands
{
    public sealed record AddDeliveryAssistantCommand:IRequest<Result>
    {
        public int AssistanWorkId { get;  set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string FrontIdentityImagePath { get; set; } = string.Empty;
        public string BackIdentityImagePath { get; set; } = string.Empty;
        public string IdentityExpirationDate { get; set; } = string.Empty;


        private class AddDeliveryAssistantCommandHandler :
            IRequestHandler<AddDeliveryAssistantCommand, Result>
        {
            private readonly IUserSession userSession;
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            private const string AssistantFoler = "Assistant";
            public AddDeliveryAssistantCommandHandler(IUserSession userSession,
                                                      INaqlahContext context,
                                                      IMediaUploader mediaUploader)
            {
                this.userSession = userSession;
                this.context = context;
                this.mediaUploader = mediaUploader;
            }
            public async Task<Result> Handle(AddDeliveryAssistantCommand request, CancellationToken cancellationToken)
            {
                var deliveryManId = await context.DeliveryMen
                                               .Where(x => x.UserId == userSession.UserId)
                                               .Select(x => x.Id)
                                               .FirstOrDefaultAsync();

                if (deliveryManId == 0)
                {
                    return Result.Failure("DeliveryMan Not Found");
                }


                var frontImage = await mediaUploader.UploadFromBase64(request.FrontIdentityImagePath,
                                                                      AssistantFoler);

                var backImage = await mediaUploader.UploadFromBase64(request.BackIdentityImagePath,
                                                                     AssistantFoler);

                DateTime identityExpirationDate;
              
                DateTime.TryParseExact(request.IdentityExpirationDate, "yyyy/MM/dd", new CultureInfo("en-US"), DateTimeStyles.None, out identityExpirationDate);


                var assitantResult = Domain.Models.Assistant.Instance(request.Name,
                                                                      request.Address,
                                                                      request.PhoneNumber,
                                                                      request.IdentityNumber,
                                                                      frontImage,
                                                                      backImage,
                                                                      identityExpirationDate,
                                                                      request.AssistanWorkId,
                                                                      deliveryManId);

                if (assitantResult.IsFailure)
                {
                    return assitantResult;
                }

                await context.Assistants.AddAsync(assitantResult.Value);
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
