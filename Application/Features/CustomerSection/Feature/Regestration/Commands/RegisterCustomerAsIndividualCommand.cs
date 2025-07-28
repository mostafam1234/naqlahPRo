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
    public sealed record RegisterCustomerAsIndividualCommand:IRequest<Result>
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; }= string.Empty;
        public string FrontIdentityImage { get; set; }= string.Empty;
        public string BackIdentityImage { get; set; }= string.Empty;
        public string Password { get; set; } = string.Empty;


        private class RegisterCustomerAsIndividualCommandHandler :
            IRequestHandler<RegisterCustomerAsIndividualCommand, Result>
        {
            private readonly IUserService userService;
            private readonly IMediaUploader mediaUploader;
            private const string CustomerFolderPrefix = "Customer";

            public RegisterCustomerAsIndividualCommandHandler(IUserService userService,
                                                              IMediaUploader mediaUploader)
            {
                this.userService = userService;
                this.mediaUploader = mediaUploader;
            }
            public async Task<Result> Handle(RegisterCustomerAsIndividualCommand request, CancellationToken cancellationToken)
            {
                var frontImage = await mediaUploader.UploadFromBase64(request.FrontIdentityImage,
                                                                      CustomerFolderPrefix);

                var backImage = await mediaUploader.UploadFromBase64(request.BackIdentityImage,
                                                                      CustomerFolderPrefix);

                var userResult = await userService.CreateCustomerUserAsIndividual(request.PhoneNumber,
                                                                                request.IdentityNumber,
                                                                                frontImage,
                                                                                backImage,
                                                                                request.Password);

                if (userResult.IsFailure)
                {
                    return Result.Failure(userResult.Error);
                }

                return Result.Success();

            }
        }

    }
}
