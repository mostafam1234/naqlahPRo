using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Commands
{
    public sealed record RegisterNewDeliveryManCommand:IRequest<Result>
    {
        public string Name { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Email { get; init; }= string.Empty;
        public string Password { get; init; } = string.Empty;

        private class RegisterNewDeliveryManCommandHandler
            : IRequestHandler<RegisterNewDeliveryManCommand, Result>
        {
            private readonly IUserService userService;

            public RegisterNewDeliveryManCommandHandler(IUserService userService)
            {
                this.userService = userService;
            }
            public async Task<Result> Handle(RegisterNewDeliveryManCommand request, CancellationToken cancellationToken)
            {
                var deliveryUser = await userService.CreateDeliveryUser(request.PhoneNumber,
                                                                        request.Email,
                                                                        request.Name,
                                                                        request.Password);

                if (deliveryUser.IsFailure)
                {
                    return Result.Failure(deliveryUser.Error);
                }

                return Result.Success();
            }
        }
    }
}
