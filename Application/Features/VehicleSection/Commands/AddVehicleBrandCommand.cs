using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.VehicleSection.Commands
{
    public sealed record AddVehicleBrandCommand : IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        private class AddVehicleBrandCommandHandler : IRequestHandler<AddVehicleBrandCommand, Result<int>>
        {
            private readonly INaqlahContext context;

            public AddVehicleBrandCommandHandler(INaqlahContext context)
            {
                this.context = context;
            }
            public async Task<Result<int>> Handle(AddVehicleBrandCommand request, CancellationToken cancellationToken)
            {
                var vehileBrand = VehicleBrand.Instance(request.ArabicName, request.EnglishName);
                var vehicleBrandValue = vehileBrand.Value;
                await context.VehicleBrands.AddAsync(vehicleBrandValue);
                var result = await context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(vehicleBrandValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }

    }
}
