using Application.Features.CustomerSection.Feature.Regestration.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Commands
{
    public sealed record CustomerLoginCommand:IRequest<Result<CustomerLoginResponse>>
    {
        public string PhoneNmber { get; set; } = string.Empty;
        public string Password { get; set; }= string.Empty;


        private class CustomerLoginCommandHandler :
            IRequestHandler<CustomerLoginCommand, Result<CustomerLoginResponse>>
        {
            private readonly IUserService userService;
            private readonly INaqlahContext context;

            public CustomerLoginCommandHandler(IUserService userService,
                                               INaqlahContext context)
            {
                this.userService = userService;
                this.context = context;
            }
            public async Task<Result<CustomerLoginResponse>> Handle(CustomerLoginCommand request, CancellationToken cancellationToken)
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNmber);
                if (user is null)
                {
                    return Result.Failure<CustomerLoginResponse>("Invalid User Name Or Passowrd");
                }

                var passwordCheck = await userService.CheckUserPassword(request.PhoneNmber,
                                                                        request.Password);

                if (passwordCheck.IsFailure)
                {
                    return Result.Failure<CustomerLoginResponse>("Invalid User Name Or Passowrd");
                }

                //if (!user.PhoneNumberConfirmed)
                //{
                //    return new CustomerLoginResponse
                //    {
                //        IsActive = false
                //    };
                //}

                var acessToken = await userService.GetAcessToken(request.PhoneNmber, request.Password);
                if (acessToken.IsFailure)
                {
                    return Result.Failure<CustomerLoginResponse>("Invalid User Name Or Password");
                }

                return new CustomerLoginResponse
                {
                    IsActive = true,
                    TokenResponse = acessToken.Value
                };
            }
        }
    }
}
