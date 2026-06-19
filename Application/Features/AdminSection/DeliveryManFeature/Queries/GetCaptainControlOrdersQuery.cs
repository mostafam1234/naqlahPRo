using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record GetCaptainControlOrdersQuery : IRequest<Result<PagedResult<GetAllOrdersDto>>>
    {
        public IReadOnlyList<int>? DeliveryManIds { get; init; }
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public OrderStatus? StatusFilter { get; init; }
        public CustomerType? CustomerTypeFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<GetCaptainControlOrdersQuery, Result<PagedResult<GetAllOrdersDto>>>
        {
            private readonly CaptainControlOrdersService _service;

            public Handler(CaptainControlOrdersService service)
            {
                _service = service;
            }

            public Task<Result<PagedResult<GetAllOrdersDto>>> Handle(
                GetCaptainControlOrdersQuery request,
                CancellationToken cancellationToken)
            {
                return _service.GetAsync(new CaptainControlOrdersRequest
                {
                    DeliveryManIds = request.DeliveryManIds,
                    Skip = request.Skip,
                    Take = request.Take,
                    SearchTerm = request.SearchTerm,
                    StatusFilter = request.StatusFilter,
                    CustomerTypeFilter = request.CustomerTypeFilter,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    LanguageId = request.LanguageId
                }, cancellationToken);
            }
        }
    }
}
