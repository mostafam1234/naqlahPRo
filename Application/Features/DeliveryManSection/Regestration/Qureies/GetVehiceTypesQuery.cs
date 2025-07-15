using Application.Features.DeliveryManSection.Regestration.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DeliveryManSection.Regestration.Qureies
{
    public sealed record GetVehiceTypesQuery:IRequest<Result<List<VehicleTypeDto>>>
    {
        private class GetVehiceTypesQueryHandler :
            IRequestHandler<GetVehiceTypesQuery, Result<List<VehicleTypeDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetVehiceTypesQueryHandler(INaqlahContext context,
                                              IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result<List<VehicleTypeDto>>> Handle(GetVehiceTypesQuery request, CancellationToken cancellationToken)
            {
                var vehicleTypes = await context.VehicleTypes
                                       .Select(x => new VehicleTypeDto
                                       {
                                           Id = x.Id,
                                           Name = userSession.LanguageId == (int)Language.Arabic ?
                                           x.ArabicName : x.EnglishName
                                       }).ToListAsync();
                return vehicleTypes;
            }
        }
    }
}
