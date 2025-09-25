using MediatR;
using System;
using Domain.InterFaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using CSharpFunctionalExtensions;

namespace Application.Features.VehicleSection.Commands
{
    public class AddVehicleTypeCommand: IRequest<Result<int>>  
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;

        private class AddVehicleTypeCommandHandler : IRequestHandler<AddVehicleTypeCommand, Result<int>>
        {
            private readonly INaqlahContext context;
            public AddVehicleTypeCommandHandler(INaqlahContext context)
            {
                this.context = context;
            }
            public async Task<Result<int>> Handle(AddVehicleTypeCommand request, CancellationToken cancellationToken)
            {
                var vehicleType = VehicleType.Instance(request.ArabicName, request.EnglishName);
                var vehicleTypeValue = vehicleType.Value;
                await context.VehicleTypes.AddAsync(vehicleTypeValue);
                var result = await context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(vehicleTypeValue.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
