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
    public sealed record DeleteVehicleBrandCommand: IRequest<Result<int>>
    {
        public int VehicleBrandId { get; set; }
        private class DeleteVehicleBrandCommandHandler : IRequestHandler<DeleteVehicleBrandCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteVehicleBrandCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteVehicleBrandCommand request, CancellationToken cancellationToken)
            {
                var VehicleBrand = await _context.VehicleBrands.AsTracking().FirstOrDefaultAsync(x => request.VehicleBrandId == x.Id);
                if (VehicleBrand == null)
                {
                    return Result.Failure<int>("Vehicle Brand Not Found");
                }
                await _context.VehicleBrands.Where(x => x.Id == request.VehicleBrandId).ExecuteDeleteAsync();
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
