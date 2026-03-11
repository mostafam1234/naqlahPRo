using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.TechSupportFeatures.Suggestions.Queries;
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
    public sealed class SuggestionsExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.Suggestions;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public SuggestionsExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllSuggestionsQuery
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
                var ws = workbook.AddWorksheet("Suggestions");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "CustomerId", "CustomerName", "CustomerMobileNumber", "CustomerAddress", "Description", "CreationDate" }
                };
                foreach (var s in data)
                {
                    rows.Add(new object[]
                    {
                        s.Id,
                        s.CustomerId.ToString(),
                        s.CustomerName ?? "",
                        s.CustomerMobileNumber ?? "",
                        s.CustomerAddress ?? "",
                        s.Description ?? "",
                        s.CreationDate
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"Suggestions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
