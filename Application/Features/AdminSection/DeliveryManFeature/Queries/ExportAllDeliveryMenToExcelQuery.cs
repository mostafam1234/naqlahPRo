using Application.Features.AdminSection.BackupFeature.Dtos;
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
    public sealed record ExportAllDeliveryMenToExcelQuery : IRequest<Result<ExportResult>>
    {
        public string? SearchTerm { get; init; }
        public bool? ActiveFilter { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public IReadOnlyList<int>? DeliveryManIds { get; init; }
        public int LanguageId { get; init; } = 1;

        private const int MaxRows = 50000;

        private sealed class Handler : IRequestHandler<ExportAllDeliveryMenToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportAllDeliveryMenToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var dataResult = await _mediator.Send(new GetAllDeliveryMenQuery
                {
                    Skip = 0,
                    Take = MaxRows,
                    SearchTerm = request.SearchTerm,
                    ActiveFilter = request.ActiveFilter,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    DeliveryManIds = request.DeliveryManIds,
                    LanguageId = request.LanguageId
                }, cancellationToken);

                if (dataResult.IsFailure)
                    return Result.Failure<ExportResult>(dataResult.Error);

                var data = dataResult.Value.Data;
                var stream = new MemoryStream();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet("DeliveryMen");
                    var rows = new List<object[]>
                    {
                        new object[]
                        {
                            "Id",
                            "FullName",
                            "PhoneNumber",
                            "Email",
                            "VehicleTypeName",
                            "VehiclePlate",
                            "DeliveryTypeName",
                            "Active",
                            "ActiveStatusName",
                            "DeliveryStateName",
                            "RegisteredAt"
                        }
                    };

                    foreach (var dm in data)
                    {
                        rows.Add(new object[]
                        {
                            dm.Id,
                            dm.FullName ?? string.Empty,
                            dm.PhoneNumber ?? string.Empty,
                            dm.Email ?? string.Empty,
                            dm.VehicleTypeName ?? string.Empty,
                            dm.VehiclePlate ?? string.Empty,
                            dm.DeliveryTypeName ?? string.Empty,
                            dm.Active,
                            dm.ActiveStatusName ?? string.Empty,
                            dm.DeliveryStateName ?? string.Empty,
                            dm.RegisteredAt
                        });
                    }

                    ws.Cell(1, 1).InsertData(rows);
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var suffix = request.DeliveryManIds is { Count: > 0 }
                    ? $"DM{string.Join("-", request.DeliveryManIds)}_"
                    : string.Empty;
                var fileName = $"DeliveryMen_{suffix}{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
