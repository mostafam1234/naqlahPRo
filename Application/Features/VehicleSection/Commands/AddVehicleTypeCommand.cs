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
        public string IconBase64 { get; set; } = string.Empty;
        public List<int> MainCategoryIds { get; set; } = new List<int>();

        private class AddVehicleTypeCommandHandler : IRequestHandler<AddVehicleTypeCommand, Result<int>>
        {
            private readonly INaqlahContext context;
            private readonly IMediaUploader mediaUploader;
            
            public AddVehicleTypeCommandHandler(INaqlahContext context, IMediaUploader mediaUploader)
            {
                this.context = context;
                this.mediaUploader = mediaUploader;
            }
            
            public async Task<Result<int>> Handle(AddVehicleTypeCommand request, CancellationToken cancellationToken)
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.IconBase64))
                {
                    return Result.Failure<int>("Icon is required");
                }

                if (request.MainCategoryIds == null || !request.MainCategoryIds.Any())
                {
                    return Result.Failure<int>("At least one main category is required");
                }

                string iconPath;
                
                // Handle icon upload
                try
                {
                    iconPath = await mediaUploader.UploadFromBase64(request.IconBase64, "vehicle-types");
                }
                catch (Exception ex)
                {
                    return Result.Failure<int>($"Failed to upload icon: {ex.Message}");
                }

                var vehicleType = VehicleType.Instance(request.ArabicName, request.EnglishName, iconPath, request.MainCategoryIds);
                if (vehicleType.IsFailure)
                {
                    return Result.Failure<int>(vehicleType.Error);
                }
                
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
