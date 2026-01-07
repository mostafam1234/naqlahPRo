using Application.Features.DeliveryManSection.NewRequests.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.DeliveryManSection.NewRequests.Queries
{
    public sealed record GetAllDeliveryMenRequestsQuery : IRequest<Result<PagedGetAllDeliveryMenRequestsPaged>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int DeliveryTypeFilter { get; set; } = 0; // 0 = All, 1 = Resident, 2 = Citizen
        public string SearchTerm { get; set; } = string.Empty; // Search in name or phone
        public int CurrentPage => Skip / Take;

        private class GetAllDeliveryMenRequestsQueryHandler : IRequestHandler<GetAllDeliveryMenRequestsQuery, Result<PagedGetAllDeliveryMenRequestsPaged>>
        {
            private readonly INaqlahContext _context;
            private readonly IReadFromAppSetting _config;
            private const string DeliveryFolderPrefix = "DeliveryMan";

            public GetAllDeliveryMenRequestsQueryHandler(INaqlahContext context, IReadFromAppSetting config)
            {
                _context = context;
                _config = config;
            }

            public async Task<Result<PagedGetAllDeliveryMenRequestsPaged>> Handle(GetAllDeliveryMenRequestsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.DeliveryMen.AsQueryable();

                if (request.DeliveryTypeFilter > 0)
                {
                    query = query.Where(x => (int)x.DeliveryType == request.DeliveryTypeFilter);
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim();
                    query = query.Where(x => x.FullName.Contains(searchTerm) || x.PhoneNumber.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);
                var baseUrl = _config.GetValue<string>("apiBaseUrl");

                var deliveryMenRequests = await query
                    .Select(x => new GetAllDeliveryMenRequestsDto
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
                        State = x.DeliveryState.ToString()
                    })
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken);

                var response = new PagedGetAllDeliveryMenRequestsPaged
                {
                    Data = deliveryMenRequests,
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