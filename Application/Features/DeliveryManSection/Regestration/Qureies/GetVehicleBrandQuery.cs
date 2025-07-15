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
    public sealed record GetVehicleBrandQuery:IRequest<Result<List<VehicleBrandDto>>>
    {
        private class GetVehicleBrandQueryHandler :
            IRequestHandler<GetVehicleBrandQuery, Result<List<VehicleBrandDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetVehicleBrandQueryHandler(INaqlahContext context,
                                               IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result<List<VehicleBrandDto>>> Handle(GetVehicleBrandQuery request, CancellationToken cancellationToken)
            {
                var brands = await context.VehicleBrands
                                        .Select(x => new VehicleBrandDto
                                        {
                                            Id = x.Id,
                                            Name = userSession.LanguageId == (int)Language.Arabic ?
                                            x.ArabicName : x.EnglishName
                                        }).ToListAsync();
                return brands;
            }
        }
    }
}
