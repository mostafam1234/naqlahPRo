using MediatR;
using Domain.InterFaces;
using Domain.Models;
using Domain.Enums;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace Application.Features.VehicleSection.Commands
{
    public class AddVehicleTypeCommand : IRequest<Result<int>>
    {
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? IconBase64 { get; set; }
        public List<int> MainCategoryIds { get; set; } = new();
        public decimal Cost { get; set; }
        public decimal ServiceFee { get; set; }
        public VehicleLoadCategory? LoadCategory { get; set; }

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
                string iconPath = string.Empty;

                if (!string.IsNullOrWhiteSpace(request.IconBase64))
                {
                    try
                    {
                        iconPath = await mediaUploader.UploadFromBase64(request.IconBase64, "vehicle-types");
                    }
                    catch (Exception)
                    {
                        return Result.Failure<int>("FailedToUploadIcon");
                    }
                }

                var vehicleType = VehicleType.Instance(
                    request.ArabicName,
                    request.EnglishName,
                    iconPath,
                    request.MainCategoryIds,
                    request.Cost,
                    request.ServiceFee,
                    request.LoadCategory);

                if (vehicleType.IsFailure)
                    return Result.Failure<int>(vehicleType.Error);

                var vehicleTypeValue = vehicleType.Value;
                await context.VehicleTypes.AddAsync(vehicleTypeValue, cancellationToken);
                var result = await context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return Result.Success(vehicleTypeValue.Id);

                return Result.Failure<int>(result.Error);
            }
        }
    }
}
