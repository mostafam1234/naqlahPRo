using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.VehicleSection.Commands
{
    public sealed record UpdateVehicleTypeCommand: IRequest<Result<int>>
    {
        public int VehicleTypeId { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;

        private class UpdateVehicleTypeCommandHandler : IRequestHandler<UpdateVehicleTypeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateVehicleTypeCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
            {
                var vehicleType = await _context.VehicleTypes.FirstOrDefaultAsync(x => request.VehicleTypeId == x.Id);
                if (vehicleType == null)
                {
                    return Result.Failure<int>("Vehicle Type Not Found");
                }
                vehicleType.Update(request.ArabicName, request.EnglishName);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(vehicleType.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
