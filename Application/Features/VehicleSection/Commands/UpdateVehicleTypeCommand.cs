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
        public string? IconBase64 { get; set; }
        public List<int> MainCategoryIds { get; set; } = new List<int>();
        public decimal Cost { get; set; }

        private class UpdateVehicleTypeCommandHandler : IRequestHandler<UpdateVehicleTypeCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IMediaUploader _mediaUploader;
            
            public UpdateVehicleTypeCommandHandler(INaqlahContext context, IMediaUploader mediaUploader)
            {
                _context = context;
                _mediaUploader = mediaUploader;
            }
            
            public async Task<Result<int>> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
            {
                var vehicleType = await _context.VehicleTypes
                    .Include(vt => vt.VehicleTypeCategoies)
                    .AsTracking()
                    .FirstOrDefaultAsync(x => request.VehicleTypeId == x.Id);
                    
                if (vehicleType == null)
                {
                    return Result.Failure<int>("Vehicle Type Not Found");
                }

                // Validation
                if (request.MainCategoryIds == null || !request.MainCategoryIds.Any())
                {
                    return Result.Failure<int>("At least one main category is required");
                }

                string iconPath = vehicleType.IconImagePath;
                
                // Handle icon upload if provided
                if (!string.IsNullOrWhiteSpace(request.IconBase64))
                {
                    try
                    {
                        iconPath = await _mediaUploader.UploadFromBase64(request.IconBase64, "vehicle-types");
                    }
                    catch (Exception ex)
                    {
                        return Result.Failure<int>($"Failed to upload icon: {ex.Message}");
                    }
                }

                // Check if we have an icon (either existing or new)
                if (string.IsNullOrWhiteSpace(iconPath))
                {
                    return Result.Failure<int>("Icon is required");
                }

                var updateResult = vehicleType.Update(request.ArabicName, request.EnglishName, iconPath, request.MainCategoryIds, request.Cost);
                if (updateResult.IsFailure)
                {
                    return Result.Failure<int>(updateResult.Error);
                }
                
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
