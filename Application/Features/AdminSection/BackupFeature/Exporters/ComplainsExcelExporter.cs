using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.TechSupportFeatures.Complains.Queries;
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
    public sealed class ComplainsExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.Complains;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public ComplainsExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllComplainsQuery
            {
                Skip = 0,
                Take = MaxRows,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }, cancellationToken);
            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("Complains");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "CustomerId", "CustomerName", "CustomerMobileNumber", "CustomerAddress", "Description", "CreationDate" }
                };
                foreach (var c in data)
                {
                    rows.Add(new object[]
                    {
                        c.Id,
                        c.CustomerId.ToString(),
                        c.CustomerName ?? "",
                        c.CustomerMobileNumber ?? "",
                        c.CustomerAddress ?? "",
                        c.Description ?? "",
                        c.CreationDate
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"Complains_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
