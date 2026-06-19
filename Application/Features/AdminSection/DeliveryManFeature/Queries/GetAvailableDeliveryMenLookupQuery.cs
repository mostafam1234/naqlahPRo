using Application.Features.AdminSection.DeliveryManFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetAvailableDeliveryMenLookupQuery : IRequest<Result<List<DeliveryManLookupDto>>>
    {
        public string? SearchTerm { get; init; }
        public bool? ActiveFilter { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetAvailableDeliveryMenLookupQuery, Result<List<DeliveryManLookupDto>>>
        {
            private readonly INaqlahContext _context;

            public Handler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<DeliveryManLookupDto>>> Handle(
                GetAvailableDeliveryMenLookupQuery request,
                CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm)
                    ? null
                    : request.SearchTerm.Trim();

                var query = _context.DeliveryMen.AsQueryable();

                if (request.ActiveFilter.HasValue)
                    query = query.Where(dm => dm.Active == request.ActiveFilter.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(dm =>
                        dm.FullName.ToLower().Contains(searchLower) ||
                        dm.PhoneNumber.Contains(searchTerm) ||
                        (searchLower.Contains("غير نشط") && !dm.Active) ||
                        (searchLower.Contains("نشط") && !searchLower.Contains("غير") && dm.Active) ||
                        (searchLower.Contains("جديد") && dm.DeliveryState == DeliveryRequesState.New) ||
                        (searchLower.Contains("موافق") && dm.DeliveryState == DeliveryRequesState.Approved) ||
                        (searchLower.Contains("مرفوض") && dm.DeliveryState == DeliveryRequesState.Rejected) ||
                        (searchLower.Contains("محظور") && dm.DeliveryState == DeliveryRequesState.Blocked) ||
                        (searchLower.Contains("معلق") && dm.DeliveryState == DeliveryRequesState.Suspended));
                }

                var deliveryMen = await query
                    .OrderByDescending(dm => dm.Active)
                    .ThenBy(dm => dm.FullName)
                    .Take(2000)
                    .Select(dm => new DeliveryManLookupDto
                    {
                        Id = dm.Id,
                        FullName = dm.FullName,
                        PhoneNumber = dm.PhoneNumber,
                        Active = dm.Active,
                        ActiveStatusName = dm.Active
                            ? (isArabic ? "نشط" : "Active")
                            : (isArabic ? "غير نشط" : "Inactive"),
                        DeliveryState = dm.DeliveryState,
                        DeliveryStateName = dm.DeliveryState == DeliveryRequesState.New
                            ? (isArabic ? "جديد" : "New")
                            : dm.DeliveryState == DeliveryRequesState.Approved
                                ? (isArabic ? "موافق عليه" : "Approved")
                                : dm.DeliveryState == DeliveryRequesState.Rejected
                                    ? (isArabic ? "مرفوض" : "Rejected")
                                    : dm.DeliveryState == DeliveryRequesState.Blocked
                                        ? (isArabic ? "محظور" : "Blocked")
                                        : dm.DeliveryState == DeliveryRequesState.Suspended
                                            ? (isArabic ? "معلق" : "Suspended")
                                            : (isArabic ? "غير محدد" : "Not specified")
                    })
                    .ToListAsync(cancellationToken);

                return Result.Success(deliveryMen);
            }
        }
    }
}
