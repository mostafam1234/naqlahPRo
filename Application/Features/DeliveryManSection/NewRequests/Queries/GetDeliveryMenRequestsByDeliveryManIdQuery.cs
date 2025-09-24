using Application.Features.DeliveryManSection.NewRequests.Dtos;
using CSharpFunctionalExtensions;
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

            public GetDeliveryMenRequestsByDeliveryManIdQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<GetDeliveryManRequestDetailsDto>> Handle(GetDeliveryMenRequestsByDeliveryManIdQuery request, CancellationToken cancellationToken)
            {
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
                        FrontIdentityImagePath = x.FrontIdenitytImagePath,
                        BackIdentityImagePath = x.BackIdenitytImagePath,
                        FrontDrivingLicenseImagePath = x.FrontDrivingLicenseImagePath,
                        BackDrivingLicenseImagePath = x.BackDrivingLicenseImagePath,
                        PersonalImagePath = x.PersonalImagePath,
                        Active = x.Active,
                        AndroidDevice = x.AndriodDevice,
                        IosDevice = x.IosDevice,
                        UserId = x.UserId,
                        VehicleId = x.VehicleId,
                        VehiclePlateNumber = x.Vehicle != null ? x.Vehicle.LicensePlateNumber : null,
                        VehicleType = x.Vehicle != null && x.Vehicle.VehicleType != null ? x.Vehicle.VehicleType.ArabicName : null,
                        VehicleModel = x.Vehicle != null && x.Vehicle.VehicleBrand != null ? x.Vehicle.VehicleBrand.ArabicName : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (deliveryMan == null)
                {
                    return Result.Failure<GetDeliveryManRequestDetailsDto>("Delivery man not found");
                }

                return Result.Success(deliveryMan);
            }
        }
    }
}
