using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record ExportOrdersByDeliveryManIdToExcelQuery : IRequest<Result<ExportResult>>
    {
        public int DeliveryManId { get; init; }
        public string? SearchTerm { get; init; }
        public OrderStatus? StatusFilter { get; init; }
        public bool? ActiveOrdersOnly { get; init; }
        public CustomerType? CustomerTypeFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportOrdersByDeliveryManIdToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task<Result<ExportResult>> Handle(
                ExportOrdersByDeliveryManIdToExcelQuery request,
                CancellationToken cancellationToken)
            {
                return _mediator.Send(new ExportCaptainControlOrdersToExcelQuery
                {
                    DeliveryManIds = new[] { request.DeliveryManId },
                    SearchTerm = request.SearchTerm,
                    StatusFilter = request.StatusFilter,
                    ActiveOrdersOnly = request.ActiveOrdersOnly,
                    CustomerTypeFilter = request.CustomerTypeFilter,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    LanguageId = request.LanguageId
                }, cancellationToken);
            }
        }
    }
}
