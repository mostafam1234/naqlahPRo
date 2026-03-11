using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.OrderFeature.Queries;
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
    public sealed class OrderPackagesExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.OrderPackages;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public OrderPackagesExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllOrderPackagesQuery
            {
                Skip = 0,
                Take = MaxRows
            }, cancellationToken);

            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("OrderPackages");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "ArabicDescription", "EnglishDescription", "MinWeightInKg", "MaxWeightInKg" }
                };
                foreach (var p in data)
                {
                    rows.Add(new object[]
                    {
                        p.Id,
                        p.ArabicDescription ?? "",
                        p.EnglishDescription ?? "",
                        p.MinWeightInKg,
                        p.MaxWeightInKg
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"OrderPackages_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
