using Application.Features.DeliveryManSection.LogIn.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.LogIn.Queries
{
    public sealed record GetDeliveryManInfoQuery:IRequest<Result<DeliveryManInfoDto>>
    {
        private class GetDeliveryManInfoQueryHandler :
            IRequestHandler<GetDeliveryManInfoQuery, Result<DeliveryManInfoDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            private readonly IReadFromAppSetting config;
            private const string DeliveryFolderPrefix = "DeliveryMan";
            public GetDeliveryManInfoQueryHandler(INaqlahContext context,
                                                  IUserSession userSession,
                                                  IReadFromAppSetting config)
            {
                this.context = context;
                this.userSession = userSession;
                this.config = config;
            }
            public async Task<Result<DeliveryManInfoDto>> Handle(GetDeliveryManInfoQuery request, CancellationToken cancellationToken)
            {
                var userId = userSession.UserId;
                var baseUrl = config.GetValue<string>("apiBaseUrl");

                var deliveryMan = await context.DeliveryMen
                                             .Where(x => x.UserId == userId)
                                             .Select(x => new DeliveryManInfoDto 
                                             { 
                                                 Id=x.Id,
                                                 Name=x.FullName,
                                                 PhoneNumber=x.PhoneNumber,
                                                 PersonalImagePath =$"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.PersonalImagePath}"

                                             }).FirstOrDefaultAsync();

                return deliveryMan;
            }
        }
    }
}
