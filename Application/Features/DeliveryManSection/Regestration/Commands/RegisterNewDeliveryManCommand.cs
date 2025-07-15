using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record RegisterNewDeliveryManCommand:IRequest<Result<DeliveryManTokenResponse>>
    {
        public string Name { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Email { get; init; }= string.Empty;
        public string Password { get; init; } = string.Empty;

        private class RegisterNewDeliveryManCommandHandler
            : IRequestHandler<RegisterNewDeliveryManCommand, Result<DeliveryManTokenResponse>>
        {
            private readonly IUserService userService;

            public RegisterNewDeliveryManCommandHandler(IUserService userService)
            {
                this.userService = userService;
            }
            public async Task<Result<DeliveryManTokenResponse>> Handle(RegisterNewDeliveryManCommand request, CancellationToken cancellationToken)
            {
                var deliveryUser = await userService.CreateDeliveryUser(request.PhoneNumber,
                                                                        request.Email,
                                                                        request.Name,
                                                                        request.Password);

                if (deliveryUser.IsFailure)
                {
                    return Result.Failure<DeliveryManTokenResponse>(deliveryUser.Error);
                }

                var tokenResponse = await userService.GetAcessToken(request.PhoneNumber,
                                                                    request.Password);

                if (tokenResponse.IsFailure)
                {
                    return Result.Failure<DeliveryManTokenResponse>("Server Error");
                }

                var deliveryResponse = new DeliveryManTokenResponse
                {
                    RequiredDeliveryInfo = true,
                    RequiredPersonalInfo = true,
                    RequiredVehicleInfo = true,
                    CarOwnerType = null,
                    TokenResponse = tokenResponse.Value
                };

                return Result.Success(deliveryResponse);
            }
        }
    }
}
