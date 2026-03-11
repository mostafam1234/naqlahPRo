using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.NotificationFeature.Queries;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using MediatR;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.BackupFeature.Exporters
{
    public sealed class NotificationsExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.Notifications;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public NotificationsExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetNotificationsQuery
            {
                Skip = 0,
                Take = MaxRows,
                LanguageId = request.LanguageId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }, cancellationToken);
            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("Notifications");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "Title", "Message", "OrderId", "NotificationType", "CreationDate", "IsRead" }
                };
                foreach (var n in data)
                {
                    rows.Add(new object[]
                    {
                        n.Id,
                        n.Title ?? "",
                        n.Message ?? "",
                        n.OrderId?.ToString() ?? "",
                        n.NotificationType.ToString(),
                        n.CreationDate,
                        n.IsRead
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"Notifications_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
