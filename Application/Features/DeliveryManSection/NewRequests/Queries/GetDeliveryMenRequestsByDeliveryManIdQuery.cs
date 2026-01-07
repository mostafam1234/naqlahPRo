using Application.Features.DeliveryManSection.NewRequests.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.DeliveryManSection.NewRequests.Queries
{
    public sealed record GetDeliveryMenRequestsByDeliveryManIdQuery : IRequest<Result<GetDeliveryManRequestDetailsDto>>
    {
        public int DeliveryManId { get; init; }

        private class GetDeliveryMenRequestsByDeliveryManIdQueryHandler : IRequestHandler<GetDeliveryMenRequestsByDeliveryManIdQuery, Result<GetDeliveryManRequestDetailsDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IReadFromAppSetting _config;
            private const string DeliveryFolderPrefix = "DeliveryMan";

            public GetDeliveryMenRequestsByDeliveryManIdQueryHandler(INaqlahContext context, IReadFromAppSetting config)
            {
                _context = context;
                _config = config;
            }

            public async Task<Result<GetDeliveryManRequestDetailsDto>> Handle(GetDeliveryMenRequestsByDeliveryManIdQuery request, CancellationToken cancellationToken)
            {
                var baseUrl = _config.GetValue<string>("apiBaseUrl");

                var deliveryMan = await _context.DeliveryMen
                    .Include(x => x.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(x => x.Vehicle)
                        .ThenInclude(v => v.VehicleBrand)
                    .Where(x => x.Id == request.DeliveryManId)
                    .Select(x => new GetDeliveryManRequestDetailsDto
                    {
                        DeliveryManId = x.Id,
                        FullName = x.FullName,
                        Address = x.Address,
                        PhoneNumber = x.PhoneNumber,
                        IdentityNumber = x.IdentityNumber,
                        IdentityExpirationDate = x.IdentityExpirationDate,
                        DrivingLicenseExpirationDate = x.DrivingLicenseExpirationDate,
                        DeliveryType = x.DeliveryType.ToString(),
                        DeliveryLicenseType = x.DeliveryLicenseType.ToString(),
                        State = x.DeliveryState.ToString(),
                        StateName = GetStateName(x.DeliveryState),
                        FrontIdentityImagePath = !string.IsNullOrEmpty(x.FrontIdenitytImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.FrontIdenitytImagePath}" 
                            : null,
                        BackIdentityImagePath = !string.IsNullOrEmpty(x.BackIdenitytImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.BackIdenitytImagePath}" 
                            : null,
                        FrontDrivingLicenseImagePath = !string.IsNullOrEmpty(x.FrontDrivingLicenseImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.FrontDrivingLicenseImagePath}" 
                            : null,
                        BackDrivingLicenseImagePath = !string.IsNullOrEmpty(x.BackDrivingLicenseImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.BackDrivingLicenseImagePath}" 
                            : null,
                        PersonalImagePath = !string.IsNullOrEmpty(x.PersonalImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.PersonalImagePath}" 
                            : null,
                        Active = x.Active,
                        AndroidDevice = x.AndriodDevice,
                        IosDevice = x.IosDevice,
                        UserId = x.UserId,
                        VehicleId = x.VehicleId,
                        VehiclePlateNumber = x.Vehicle != null ? x.Vehicle.LicensePlateNumber : null,
                        VehicleType = x.Vehicle != null && x.Vehicle.VehicleType != null ? x.Vehicle.VehicleType.ArabicName : null,
                        VehicleModel = x.Vehicle != null && x.Vehicle.VehicleBrand != null ? x.Vehicle.VehicleBrand.ArabicName : null,
                        VehicleFrontImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.FrontImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.FrontImagePath}" 
                            : null,
                        VehicleSideImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.SideImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.SideImagePath}" 
                            : null,
                        VehicleFrontLicenseImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.FrontLicenseImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.FrontLicenseImagePath}" 
                            : null,
                        VehicleBackLicenseImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.BackLicenseImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.BackLicenseImagePath}" 
                            : null,
                        VehicleFrontInsuranceImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.FrontInsuranceImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.FrontInsuranceImagePath}" 
                            : null,
                        VehicleBackInsuranceImagePath = x.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.BackInsuranceImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.Vehicle.BackInsuranceImagePath}" 
                            : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryMan == null)
                {
                    return Result.Failure<GetDeliveryManRequestDetailsDto>("Delivery man not found");
                }

                return Result.Success(deliveryMan);
            }

            private static string GetStateName(DeliveryRequesState state)
            {
                return state switch
                {
                    DeliveryRequesState.New => "جديد",
                    DeliveryRequesState.Approved => "موافق عليه",
                    DeliveryRequesState.Rejected => "مرفوض",
                    DeliveryRequesState.Blocked => "محظور",
                    DeliveryRequesState.Suspended => "معلق",
                    _ => state.ToString()
                };
            }
        }
    }
}
