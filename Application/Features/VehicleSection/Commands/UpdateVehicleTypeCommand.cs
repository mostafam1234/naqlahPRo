using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Application.Features.VehicleSection.Commands
{
    public sealed record UpdateVehicleTypeCommand : IRequest<Result<int>>
    {
        public int VehicleTypeId { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? IconBase64 { get; set; }
        public List<int> MainCategoryIds { get; set; } = new();
        public decimal Cost { get; set; }
        public decimal ServiceFee { get; set; }
        public VehicleLoadCategory? LoadCategory { get; set; }

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
                    .FirstOrDefaultAsync(x => request.VehicleTypeId == x.Id, cancellationToken);

                if (vehicleType == null)
                    return Result.Failure<int>("VehicleTypeNotFound");

                string iconPath = vehicleType.IconImagePath;

                if (!string.IsNullOrWhiteSpace(request.IconBase64))
                {
                    try
                    {
                        iconPath = await _mediaUploader.UploadFromBase64(request.IconBase64, "vehicle-types");
                    }
                    catch (Exception)
                    {
                        return Result.Failure<int>("FailedToUploadIcon");
                    }
                }

                var updateResult = vehicleType.Update(
                    request.ArabicName,
                    request.EnglishName,
                    iconPath,
                    request.MainCategoryIds,
                    request.Cost,
                    request.ServiceFee,
                    request.LoadCategory);

                if (updateResult.IsFailure)
                    return Result.Failure<int>(updateResult.Error);

                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return Result.Success(vehicleType.Id);

                return Result.Failure<int>(result.Error);
            }
        }
    }
}
