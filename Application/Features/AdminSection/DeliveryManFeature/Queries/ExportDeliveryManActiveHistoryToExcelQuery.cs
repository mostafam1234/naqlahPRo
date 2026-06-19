using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using ClosedXML.Excel;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.DeliveryManFeature.Queries
{
    public sealed record ExportDeliveryManActiveHistoryToExcelQuery : IRequest<Result<ExportResult>>
    {
        public int DeliveryManId { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportDeliveryManActiveHistoryToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportDeliveryManActiveHistoryToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var dataResult = await _mediator.Send(new GetDeliveryManActiveHistoryQuery
                {
                    DeliveryManId = request.DeliveryManId,
                    LanguageId = request.LanguageId
                }, cancellationToken);

                if (dataResult.IsFailure)
                    return Result.Failure<ExportResult>(dataResult.Error);

                var data = dataResult.Value;
                var isArabic = request.LanguageId == 1;
                var rows = data.History
                    .OrderBy(x => x.ChangedAt)
                    .Select(item => new object[]
                    {
                        data.FullName,
                        item.ActiveStatusName,
                        item.ChangedAt,
                        item.ChangedByUserName ?? (isArabic ? "النظام" : "System")
                    })
                    .ToList();

                var stream = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet(isArabic ? "سجل النشاط" : "ActiveHistory");
                    var header = isArabic
                        ? new object[] { "اسم الكابتن", "الحالة", "تاريخ ووقت التغيير", "تم التغيير بواسطة" }
                        : new object[] { "CaptainName", "Status", "ChangedAt", "ChangedBy" };

                    var allRows = new List<object[]> { header };
                    allRows.AddRange(rows);
                    ws.Cell(1, 1).InsertData(allRows);

                    if (rows.Count > 0)
                    {
                        ws.Column(3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var safeName = string.IsNullOrWhiteSpace(data.FullName)
                    ? $"DM{data.DeliveryManId}"
                    : new string(data.FullName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray()).Trim();
                if (string.IsNullOrWhiteSpace(safeName))
                    safeName = $"DM{data.DeliveryManId}";

                var fileName = $"CaptainActiveHistory_{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
