using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetAllDeliveryMenQuery : IRequest<Result<PagedResult<GetAllDeliveryMenDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public int LanguageId { get; init; } = 1;

        private class GetAllDeliveryMenQueryHandler : IRequestHandler<GetAllDeliveryMenQuery, Result<PagedResult<GetAllDeliveryMenDto>>>
        {
            private readonly INaqlahContext _context;

            public GetAllDeliveryMenQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<GetAllDeliveryMenDto>>> Handle(GetAllDeliveryMenQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                var query = from dm in _context.DeliveryMen
                           join user in _context.Users on dm.UserId equals user.Id
                           join vehicle in _context.DeliveryVehicles on dm.Id equals vehicle.DeliveryManId into vehicleGroup
                           from vehicle in vehicleGroup.DefaultIfEmpty()
                           join vehicleType in _context.VehicleTypes on vehicle.VehicleTypeId equals vehicleType.Id into vehicleTypeGroup
                           from vt in vehicleTypeGroup.DefaultIfEmpty()
                           where dm.DeliveryState == DeliveryRequesState.Approved
                           select new
                           {
                               DeliveryMan = dm,
                               User = user,
                               Vehicle = vehicle,
                               VehicleType = vt
                           };

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.DeliveryMan.FullName.ToLower().Contains(searchLower) ||
                        x.DeliveryMan.PhoneNumber.Contains(searchLower) ||
                        (x.User.Email != null && x.User.Email.ToLower().Contains(searchLower))
                    );
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var deliveryMen = await query
                    .OrderByDescending(x => x.DeliveryMan.Id)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new GetAllDeliveryMenDto
                    {
                        Id = x.DeliveryMan.Id,
                        FullName = x.DeliveryMan.FullName,
                        PhoneNumber = x.DeliveryMan.PhoneNumber,
                        Email = x.User.Email ?? string.Empty,
                        PersonalImagePath = x.DeliveryMan.PersonalImagePath,
                        VehicleTypeName = x.VehicleType != null ? (isArabic ? x.VehicleType.ArabicName : x.VehicleType.EnglishName) : string.Empty,
                        VehiclePlate = x.Vehicle != null ? x.Vehicle.LicensePlateNumber : string.Empty,
                        DeliveryTypeName = isArabic 
                            ? (x.DeliveryMan.DeliveryType == DeliveryType.Resident ? "مقيم" : "مواطن")
                            : (x.DeliveryMan.DeliveryType == DeliveryType.Resident ? "Resident" : "Citizen"),
                        Active = x.DeliveryMan.Active
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.Take);
                var result = new PagedResult<GetAllDeliveryMenDto>
                {
                    Data = deliveryMen,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Result.Success(result);
            }
        }
    }
}

