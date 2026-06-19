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

                var x = await _context.DeliveryMen
                    .AsSplitQuery()
                    .Include(dm => dm.Vehicle!)
                        .ThenInclude(v => v.VehicleType)
                    .Include(dm => dm.Vehicle!)
                        .ThenInclude(v => v.VehicleBrand)
                    .Include(dm => dm.Vehicle!)
                        .ThenInclude(v => v.Resident)
                    .Include(dm => dm.Vehicle!)
                        .ThenInclude(v => v.Company)
                    .Include(dm => dm.Vehicle!)
                        .ThenInclude(v => v.Renter)
                    .Include(dm => dm.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dm => dm.Id == request.DeliveryManId, cancellationToken);

                if (x == null)
                    return Result.Failure<GetDeliveryManRequestDetailsDto>("DeliveryManNotFound");

                string? Img(string? path) => !string.IsNullOrEmpty(path)
                    ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{path}"
                    : null;

                var vehicle = x.Vehicle;
                string? ownerName = null;
                string? ownerIdentity = null;
                string? commercialNumber = null;
                string? ownerFrontId = null;
                string? ownerBackId = null;
                string? commercialImg = null;
                string? rentContract = null;
                string? taxNumber = null;
                string? taxCertificateImg = null;
                string? ownerBankAccount = null;

                if (vehicle != null)
                {
                    switch (vehicle.VehicleOwnerType)
                    {
                        case VehicleOwnerType.Resident when vehicle.Resident != null:
                            ownerName = vehicle.Resident.CitizenName;
                            ownerIdentity = vehicle.Resident.IdentityNumber;
                            ownerFrontId = Img(vehicle.Resident.FrontIdentityImagePath);
                            ownerBackId = Img(vehicle.Resident.BackIdentityImagePath);
                            ownerBankAccount = vehicle.Resident.BankAccountNumber;
                            break;
                        case VehicleOwnerType.Company when vehicle.Company != null:
                            ownerName = vehicle.Company.CompanyName;
                            commercialNumber = vehicle.Company.CommercialRecordNumber;
                            commercialImg = Img(vehicle.Company.RecordImagePath);
                            taxNumber = vehicle.Company.TaxNumber;
                            taxCertificateImg = Img(vehicle.Company.TaxCertificateImagePath);
                            ownerBankAccount = vehicle.Company.BankAccountNumber;
                            break;
                        case VehicleOwnerType.Renter when vehicle.Renter != null:
                            ownerName = vehicle.Renter.CitizenName;
                            ownerIdentity = vehicle.Renter.IdentityNumber;
                            ownerFrontId = Img(vehicle.Renter.FrontIdentityImagePath);
                            ownerBackId = Img(vehicle.Renter.BackIdentityImagePath);
                            rentContract = Img(vehicle.Renter.RentContractImagePath);
                            ownerBankAccount = vehicle.Renter.BankAccountNumber;
                            break;
                    }
                }

                var dto = new GetDeliveryManRequestDetailsDto
                {
                    DeliveryManId = x.Id,
                    FullName = x.FullName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IdentityNumber = x.IdentityNumber,
                    BirthDate = x.BirthDate,
                    IdentityExpirationDate = x.IdentityExpirationDate,
                    DrivingLicenseExpirationDate = x.DrivingLicenseExpirationDate,
                    DeliveryType = x.DeliveryType.ToString(),
                    DeliveryLicenseType = x.DeliveryLicenseType.ToString(),
                    State = x.DeliveryState.ToString(),
                    StateName = GetStateName(x.DeliveryState),
                    FrontIdentityImagePath = Img(x.FrontIdenitytImagePath),
                    BackIdentityImagePath = Img(x.BackIdenitytImagePath),
                    FrontDrivingLicenseImagePath = Img(x.FrontDrivingLicenseImagePath),
                    BackDrivingLicenseImagePath = Img(x.BackDrivingLicenseImagePath),
                    PersonalImagePath = Img(x.PersonalImagePath),
                    Active = x.Active,
                    AndroidDevice = x.AndriodDevice,
                    IosDevice = x.IosDevice,
                    UserId = x.UserId,
                    Email = x.User?.Email ?? string.Empty,
                    VehicleId = x.VehicleId,
                    VehiclePlateNumber = vehicle?.LicensePlateNumber,
                    VehicleType = vehicle?.VehicleType?.ArabicName,
                    VehicleTypeId = vehicle?.VehicleTypeId,
                    VehicleModel = vehicle?.VehicleBrand?.ArabicName,
                    VehicleBrandId = vehicle?.VehicleBrandId,
                    VehicleOwnerTypeId = vehicle != null ? (int)vehicle.VehicleOwnerType : null,
                    VehicleOwnerName = ownerName,
                    VehicleOwnerIdentityNumber = ownerIdentity,
                    CommercialRecordNumber = commercialNumber,
                    VehicleLicenseExpirationDate = vehicle?.LicenseExpirationDate,
                    VehicleInsuranceExpirationDate = vehicle?.InSuranceExpirationDate,
                    VehicleFrontImagePath = vehicle != null ? Img(vehicle.FrontImagePath) : null,
                    VehicleSideImagePath = vehicle != null ? Img(vehicle.SideImagePath) : null,
                    VehicleFrontLicenseImagePath = vehicle != null ? Img(vehicle.FrontLicenseImagePath) : null,
                    VehicleBackLicenseImagePath = vehicle != null ? Img(vehicle.BackLicenseImagePath) : null,
                    VehicleFrontInsuranceImagePath = vehicle != null ? Img(vehicle.FrontInsuranceImagePath) : null,
                    VehicleBackInsuranceImagePath = vehicle != null ? Img(vehicle.BackInsuranceImagePath) : null,
                    OwnerFrontIdentityImagePath = ownerFrontId,
                    OwnerBackIdentityImagePath = ownerBackId,
                    CommercialRecordImagePath = commercialImg,
                    RentContractImagePath = rentContract,
                    TaxNumber = taxNumber,
                    TaxCertificateImagePath = taxCertificateImg,
                    OwnerBankAccountNumber = ownerBankAccount,
                    HasIncompleteRegistration = x.HasIncompleteRegistration
                };

                return Result.Success(dto);
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
