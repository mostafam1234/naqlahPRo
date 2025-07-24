using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.LogIn.Commands
{
    public sealed record DeliveryManLogInCommand : IRequest<Result<DeliveryManTokenResponse>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;


        private class DeliveryManLogInCommandHandler :
            IRequestHandler<DeliveryManLogInCommand, Result<DeliveryManTokenResponse>>
        {
            private readonly UserManager<User> userManager;
            private readonly IUserService userService;
            private readonly INaqlahContext context;

            public DeliveryManLogInCommandHandler(UserManager<User> userManager,
                                                  IUserService userService,
                                                  INaqlahContext context)
            {
                this.userManager = userManager;
                this.userService = userService;
                this.context = context;
            }
            public async Task<Result<DeliveryManTokenResponse>> Handle(DeliveryManLogInCommand request,
                                                                       CancellationToken cancellationToken)
            {

                var tokenResponse = await userService.GetAcessToken(request.UserName, request.Password);
                if (tokenResponse.IsFailure)
                {
                    return Result.Failure<DeliveryManTokenResponse>(tokenResponse.Error);
                }

                var deliveryToneResponse = new DeliveryManTokenResponse
                {
                    RequiredCarOwnerInfo = true,
                    RequiredPersonalInfo = true,
                    RequiredVehicleInfo = true,
                    TokenResponse = tokenResponse.Value
                };

                var deliveryManUser = await context.Users
                                                   .FirstOrDefaultAsync(x => x.UserName == request.UserName,
                                                                        cancellationToken);

                if (deliveryManUser is null)
                {
                    return deliveryToneResponse;
                }

                var deliveryMan = await context.DeliveryMen
                                               .Include(x => x.Vehicle)
                                               .FirstOrDefaultAsync(x => x.UserId == deliveryManUser.Id);

                if (deliveryMan is null)
                {
                    return deliveryToneResponse;
                }


                if (deliveryMan.DeliveryType == DeliveryType.Citizen ||
                    deliveryMan.DeliveryType == DeliveryType.Resident)
                {
                    deliveryToneResponse.RequiredPersonalInfo = false;
                }



                if (deliveryMan.Vehicle is not null)
                {
                    var ownerCarType = deliveryMan.Vehicle.VehicleOwnerType;
                    deliveryToneResponse.CarOwnerType = (int)ownerCarType;
                    deliveryToneResponse.RequiredVehicleInfo = false;

                     var carOwnerTypeExist = ownerCarType == VehicleOwnerType.Resident ?
                    await context.Residents.AnyAsync(x => x.DeliveryVehicleId == deliveryMan.Vehicle.Id) :
                    ownerCarType == VehicleOwnerType.Company ?
                    await context.Companies.AnyAsync(x => x.DeliveryVehicleId == deliveryMan.Vehicle.Id) :
                    await context.Renters.AnyAsync(x => x.DeliveryVehicleId == deliveryMan.Vehicle.Id);


                deliveryToneResponse.RequiredCarOwnerInfo = !carOwnerTypeExist;
                }




                return deliveryToneResponse;


            }
        }
    }
}
