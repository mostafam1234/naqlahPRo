using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.OrderFeature.Dtos;
using Application.Features.AdminSection.OrderFeature.Queries;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using MediatR;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.BackupFeature.Exporters
{
    public sealed class OrdersExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.Orders;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public OrdersExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllOrdersQuery
            {
                Skip = 0,
                Take = MaxRows,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                LanguageId = request.LanguageId
            }, cancellationToken);

            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("Orders");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "OrderNumber", "CreatedDate", "Status", "OrderType", "Total", "CustomerName", "CustomerPhone", "CustomerType" }
                };
                foreach (var o in data)
                {
                    rows.Add(new object[]
                    {
                        o.Id,
                        o.OrderNumber ?? "",
                        o.CreatedDate,
                        o.StatusName ?? "",
                        o.OrderTypeName ?? "",
                        o.Total,
                        o.CustomerName ?? "",
                        o.CustomerPhone ?? "",
                        o.CustomerTypeName ?? ""
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
