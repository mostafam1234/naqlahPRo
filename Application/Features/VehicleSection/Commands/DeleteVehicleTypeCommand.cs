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
    public sealed record DeleteVehicleTypeCommand : IRequest<Result<int>>
    {
        public int VehicleTypeId { get; set; }
        private class DeleteVehicleTypeCommandHandler : IRequestHandler<DeleteVehicleTypeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            public DeleteVehicleTypeCommandHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(DeleteVehicleTypeCommand request, CancellationToken cancellationToken)
            {
                var VehicleType = await _context.VehicleTypes.AsTracking().FirstOrDefaultAsync(x => request.VehicleTypeId == x.Id);
                if (VehicleType == null)
                {
                    return Result.Failure<int>("Vehicle Type Not Found");
                }
                await _context.VehicleTypes.Where(x => x.Id == request.VehicleTypeId).ExecuteDeleteAsync();
                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                {
                    return Result.Success(VehicleType.Id);
                }
                return Result.Failure<int>(result.Error);
            }
        }   
    }
}
