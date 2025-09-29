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
    public sealed record UpdateVehicleBrandCommand: IRequest<Result<int>>
    {
        public int VehicleBrandId { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;

        private class UpdateVehicleBrandCommandHandler : IRequestHandler<UpdateVehicleBrandCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public UpdateVehicleBrandCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(UpdateVehicleBrandCommand request, CancellationToken cancellationToken)
            {
                var VehicleBrand = await _context.VehicleBrands.AsTracking().FirstOrDefaultAsync(x => request.VehicleBrandId == x.Id );
                if (VehicleBrand == null)
                {
                    return Result.Failure<int>("Vehicle Brand Not Found");
                }
                VehicleBrand.Update(request.ArabicName, request.EnglishName);
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(VehicleBrand.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }
    }
}
