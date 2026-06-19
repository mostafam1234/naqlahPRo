using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using Application.Shared.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public bool? ActiveFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public IReadOnlyList<int>? DeliveryManIds { get; init; }
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
                var deliveryManIds = request.DeliveryManIds?
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (deliveryManIds is { Count: > 0 })
                {
                    var validCount = await _context.DeliveryMen
                        .CountAsync(dm => deliveryManIds.Contains(dm.Id), cancellationToken);

                    if (validCount != deliveryManIds.Count)
                        return Result.Failure<PagedResult<GetAllDeliveryMenDto>>("DeliveryManNotFound");
                }

                var isArabic = request.LanguageId == (int)Language.Arabic;

                var query = from dm in _context.DeliveryMen
                            join user in _context.Users on dm.UserId equals user.Id
                            join vehicle in _context.DeliveryVehicles on dm.Id equals vehicle.DeliveryManId into vehicleGroup
                            from vehicle in vehicleGroup.DefaultIfEmpty()
                            join vehicleType in _context.VehicleTypes on vehicle.VehicleTypeId equals vehicleType.Id into vehicleTypeGroup
                            from vt in vehicleTypeGroup.DefaultIfEmpty()
                            select new
                            {
                                DeliveryMan = dm,
                                User = user,
                                Vehicle = vehicle,
                                VehicleType = vt
                            };

                if (deliveryManIds is { Count: > 0 })
                    query = query.Where(x => deliveryManIds.Contains(x.DeliveryMan.Id));

                if (request.ActiveFilter.HasValue)
                    query = query.Where(x => x.DeliveryMan.Active == request.ActiveFilter.Value);

                if (request.FromDate.HasValue)
                {
                    var fromDate = request.FromDate.Value.Date.ToUniversalTime();
                    query = query.Where(x => x.DeliveryMan.RegisteredAt >= fromDate);
                }

                if (request.ToDate.HasValue)
                {
                    var toDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(x => x.DeliveryMan.RegisteredAt <= toDate);
                }

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim();
                    var searchLower = searchTerm.ToLower();

                    query = query.Where(x =>
                        x.DeliveryMan.FullName.ToLower().Contains(searchLower) ||
                        x.DeliveryMan.PhoneNumber.Contains(searchTerm) ||
                        (x.User.Email != null && x.User.Email.ToLower().Contains(searchLower)) ||
                        (searchLower.Contains("غير نشط") && !x.DeliveryMan.Active) ||
                        (searchLower.Contains("نشط") && !searchLower.Contains("غير") && x.DeliveryMan.Active) ||
                        (searchLower.Contains("جديد") && x.DeliveryMan.DeliveryState == DeliveryRequesState.New) ||
                        (searchLower.Contains("موافق") && x.DeliveryMan.DeliveryState == DeliveryRequesState.Approved) ||
                        (searchLower.Contains("مرفوض") && x.DeliveryMan.DeliveryState == DeliveryRequesState.Rejected) ||
                        (searchLower.Contains("محظور") && x.DeliveryMan.DeliveryState == DeliveryRequesState.Blocked) ||
                        (searchLower.Contains("معلق") && x.DeliveryMan.DeliveryState == DeliveryRequesState.Suspended));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var deliveryMen = await query
                    .OrderByDescending(x => x.DeliveryMan.RegisteredAt)
                    .ThenByDescending(x => x.DeliveryMan.Active)
                    .ThenBy(x => x.DeliveryMan.FullName)
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
                        Active = x.DeliveryMan.Active,
                        ActiveStatusName = DeliveryManDisplayLabels.GetActiveStatusName(x.DeliveryMan.Active, request.LanguageId),
                        DeliveryState = x.DeliveryMan.DeliveryState,
                        DeliveryStateName = DeliveryManDisplayLabels.GetDeliveryStateName(x.DeliveryMan.DeliveryState, request.LanguageId),
                        RegisteredAt = x.DeliveryMan.RegisteredAt
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = request.Take > 0 ? (int)Math.Ceiling(totalCount / (double)request.Take) : 0;
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
