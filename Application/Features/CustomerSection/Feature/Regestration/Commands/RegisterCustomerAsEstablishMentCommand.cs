using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Commands
{
    public sealed record RegisterCustomerAsEstablishMentCommand:IRequest<Result>
    {
        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get;  set; }= string.Empty;
        public string RecoredImage { get;  set; }= string.Empty;
        public string TaxRegistrationNumber { get;  set; }= string.Empty;
        public string TaxRegistrationImage { get;  set; }= string.Empty;
        public string Address { get;  set; }= string.Empty;
        public string RepresentitveName { get; set; } = string.Empty;
        public string RepresentitvePhoneNumber { get; set; } = string.Empty;
        public string RepresentitveFrontIdentityNumberImage { get; set; } = string.Empty;
        public string RepresentitveBackIdentityNumberImage { get;  set; }=string.Empty;
        public string Password { get; set; } = string.Empty;

        private class RegisterCustomerAsEstablishMentCommandHandler :
            IRequestHandler<RegisterCustomerAsEstablishMentCommand, Result>
        {
            private readonly IUserService userService;
            private readonly IMediaUploader mediaUploader;
            private const string CustomerFolderPrefix = "Customer";
            public RegisterCustomerAsEstablishMentCommandHandler(IUserService userService,
                                                                 IMediaUploader mediaUploader)
            {
                this.userService = userService;
                this.mediaUploader = mediaUploader;
            }
            public async Task<Result> Handle(RegisterCustomerAsEstablishMentCommand request, CancellationToken cancellationToken)
            {
                var frontImage = await mediaUploader.UploadFromBase64
                                                      (request.RepresentitveFrontIdentityNumberImage,
                                                       CustomerFolderPrefix);

                var backImage = await mediaUploader.UploadFromBase64(request.RepresentitveBackIdentityNumberImage,
                  CustomerFolderPrefix);

                var recordImage = await mediaUploader.UploadFromBase64(request.RecoredImage,
                                                                       CustomerFolderPrefix);

                var taxRegestratioImage = await mediaUploader.UploadFromBase64(request.TaxRegistrationImage,
                                                                               CustomerFolderPrefix);

                var user = await userService.CreateCustomerUserAsEstablishMent(request.MobileNumber,
                                                                             request.Name,
                                                                             recordImage,
                                                                             request.TaxRegistrationNumber,
                                                                             taxRegestratioImage,
                                                                             request.Address,
                                                                             request.RepresentitveName,
                                                                             request.RepresentitvePhoneNumber,
                                                                             frontImage,
                                                                             backImage,
                                                                             request.Password);

                if (user.IsFailure)
                {
                    return Result.Failure(user.Error);
                }
                return Result.Success();
            }
        }

    }
}
