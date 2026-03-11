using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.DeliveryManFeature.Queries;
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
    public sealed class DeliveryMenExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.DeliveryMen;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public DeliveryMenExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllDeliveryMenQuery { Skip = 0, Take = MaxRows, LanguageId = request.LanguageId }, cancellationToken);
            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("DeliveryMen");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "FullName", "PhoneNumber", "Email", "VehicleTypeName", "VehiclePlate", "DeliveryTypeName", "Active" }
                };
                foreach (var d in data)
                {
                    rows.Add(new object[]
                    {
                        d.Id,
                        d.FullName ?? "",
                        d.PhoneNumber ?? "",
                        d.Email ?? "",
                        d.VehicleTypeName ?? "",
                        d.VehiclePlate ?? "",
                        d.DeliveryTypeName ?? "",
                        d.Active
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"DeliveryMen_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
