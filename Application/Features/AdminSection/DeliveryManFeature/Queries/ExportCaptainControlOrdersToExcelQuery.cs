using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using ClosedXML.Excel;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record ExportCaptainControlOrdersToExcelQuery : IRequest<Result<ExportResult>>
    {
        public IReadOnlyList<int>? DeliveryManIds { get; init; }
        public string? SearchTerm { get; init; }
        public Domain.Enums.OrderStatus? StatusFilter { get; init; }
        public bool? ActiveOrdersOnly { get; init; }
        public Domain.Enums.CustomerType? CustomerTypeFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private const int MaxRows = 50000;

        private sealed class Handler : IRequestHandler<ExportCaptainControlOrdersToExcelQuery, Result<ExportResult>>
        {
            private readonly CaptainControlOrdersService _service;

            public Handler(CaptainControlOrdersService service)
            {
                _service = service;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportCaptainControlOrdersToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var dataResult = await _service.GetAllForExportAsync(new CaptainControlOrdersRequest
                {
                    DeliveryManIds = request.DeliveryManIds,
                    SearchTerm = request.SearchTerm,
                    StatusFilter = request.StatusFilter,
                    ActiveOrdersOnly = request.ActiveOrdersOnly,
                    CustomerTypeFilter = request.CustomerTypeFilter,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    LanguageId = request.LanguageId
                }, MaxRows, cancellationToken);

                if (dataResult.IsFailure)
                    return Result.Failure<ExportResult>(dataResult.Error);

                var data = dataResult.Value;
                var stream = new MemoryStream();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet("CaptainOrders");
                    var rows = new List<object[]>
                    {
                        new object[]
                        {
                            "Id",
                            "OrderNumber",
                            "CreatedDate",
                            "Status",
                            "CustomerName",
                            "CustomerPhone",
                            "CustomerType",
                            "DeliveryManName",
                            "DeliveryManPhone",
                            "OrderType",
                            "Total"
                        }
                    };

                    foreach (var order in data)
                    {
                        rows.Add(new object[]
                        {
                            order.Id,
                            order.OrderNumber ?? string.Empty,
                            order.CreatedDate,
                            order.StatusName ?? OrderDisplayLabels.GetOrderStatusName(order.Status, request.LanguageId),
                            order.CustomerName ?? string.Empty,
                            order.CustomerPhone ?? string.Empty,
                            order.CustomerTypeName ?? OrderDisplayLabels.GetCustomerTypeName(order.CustomerType, request.LanguageId),
                            order.DeliveryManName ?? string.Empty,
                            order.DeliveryManPhone ?? string.Empty,
                            order.OrderTypeName ?? OrderDisplayLabels.GetOrderTypeName(order.OrderType, request.LanguageId),
                            order.Total
                        });
                    }

                    ws.Cell(1, 1).InsertData(rows);
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var suffix = request.DeliveryManIds is { Count: > 0 }
                    ? $"DM{string.Join("-", request.DeliveryManIds)}_"
                    : string.Empty;
                var fileName = $"CaptainOrders_{suffix}{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
