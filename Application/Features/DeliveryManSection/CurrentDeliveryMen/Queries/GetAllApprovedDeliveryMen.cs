using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Queries
{
    public sealed record GetAllApprovedDeliveryMenQuery : IRequest<Result<PagedGetAllApprovedDeliveryMenPaged>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int DeliveryTypeFilter { get; set; } = 0; // 0 = All, 1 = Resident, 2 = Citizen
        public string SearchTerm { get; set; } = string.Empty; // Search in name or phone
        public int CurrentPage => Skip / Take;

        private class GetAllApprovedDeliveryMenQueryHandler : IRequestHandler<GetAllApprovedDeliveryMenQuery, Result<PagedGetAllApprovedDeliveryMenPaged>>
        {
            private readonly INaqlahContext _context;
            private readonly IReadFromAppSetting _config;
            private const string DeliveryFolderPrefix = "DeliveryMan";

            public GetAllApprovedDeliveryMenQueryHandler(INaqlahContext context, IReadFromAppSetting config)
            {
                _context = context;
                _config = config;
            }

            public async Task<Result<PagedGetAllApprovedDeliveryMenPaged>> Handle(GetAllApprovedDeliveryMenQuery request, CancellationToken cancellationToken)
            {
                var approvedQuery = _context.DeliveryMen.Where(x => x.DeliveryState == DeliveryRequesState.Approved);

                // Apply delivery type filter if not "All" (0)
                if (request.DeliveryTypeFilter > 0)
                {
                    approvedQuery = approvedQuery.Where(x => (int)x.DeliveryType == request.DeliveryTypeFilter);
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim();
                    approvedQuery = approvedQuery.Where(x => x.FullName.Contains(searchTerm) || x.PhoneNumber.Contains(searchTerm));
                }

                var totalCount = await approvedQuery.CountAsync(cancellationToken);
                var baseUrl = _config.GetValue<string>("apiBaseUrl");

                var approvedDeliveryMen = await approvedQuery
                    .Select(x => new GetAllApprovedDeliveryMenDto
                    {
                        DeliveryManId = x.Id,
                        FullName = x.FullName,
                        PhoneNumber = x.PhoneNumber,
                        Address = x.Address,
                        IdentityNumber = x.IdentityNumber,
                        PersonalImagePath = !string.IsNullOrEmpty(x.PersonalImagePath) 
                            ? $"{baseUrl}/ImageBank/{DeliveryFolderPrefix}_{x.Id}/{x.PersonalImagePath}" 
                            : null,
                        DeliveryType = x.DeliveryType.ToString(),
                        State = (int)x.DeliveryState,
                        Active = x.Active,
                    })
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken);

                var response = new PagedGetAllApprovedDeliveryMenPaged
                {
                    Data = approvedDeliveryMen,
                    TotalCount = totalCount,
                    CurrentPage = request.CurrentPage,
                    PageSize = request.Take,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.Take)
                };

                return Result.Success(response);
            }
        }
    }
}
