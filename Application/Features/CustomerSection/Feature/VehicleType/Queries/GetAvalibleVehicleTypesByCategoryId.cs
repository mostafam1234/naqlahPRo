using Application.Features.CustomerSection.Feature.VehicleType.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.VehicleType.Queries
{
    public sealed record GetAvalibleVehicleTypesByCategoryId:
        IRequest<Result<List<VehicleTypeDto>>>
    {
        public int CategoryId { get; set; }

        private class GetAvalibleVehicleTypesByCategoryIdHandler :
            IRequestHandler<GetAvalibleVehicleTypesByCategoryId,
                           Result<List<VehicleTypeDto>>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;
            public GetAvalibleVehicleTypesByCategoryIdHandler(INaqlahContext context,
                                                   IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result<List<VehicleTypeDto>>> Handle(GetAvalibleVehicleTypesByCategoryId request, CancellationToken cancellationToken)
            {
                var languageId = userSession.LanguageId;
                var vehicleTypes = await context.VehicleTypes
                                                .Where(x =>x.VehicleTypeCategoies.Any(v=>v.MainCategoryId==request.CategoryId))
                                                .Select(x => new VehicleTypeDto
                                                {
                                                    Id = x.Id,
                                                    Name = languageId == (int)Domain.Enums.Language.Arabic ?
                                                    x.ArabicName : x.EnglishName
                                                })
                                                .ToListAsync(cancellationToken);
                return Result.Success(vehicleTypes);
            }
        }
    }
}
