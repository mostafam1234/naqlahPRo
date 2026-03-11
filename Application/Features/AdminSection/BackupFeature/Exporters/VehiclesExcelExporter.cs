using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.VehicleSection.Queries;
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
    public sealed class VehiclesExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.Vehicles;
        private const int MaxRows = 10000;
        private readonly IMediator _mediator;

        public VehiclesExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var typesResult = await _mediator.Send(new GetVehiclesTypesQueryForDisplaying { Skip = 0, Take = MaxRows }, cancellationToken);
                if (typesResult.IsSuccess && typesResult.Value.Data.Count > 0)
                {
                    var wsTypes = workbook.AddWorksheet("VehicleTypes");
                    var rows = new List<object[]> { new object[] { "Id", "ArabicName", "EnglishName", "Cost" } };
                    foreach (var t in typesResult.Value.Data)
                    {
                        rows.Add(new object[] { t.Id, t.ArabicName ?? "", t.EnglishName ?? "", t.Cost });
                    }
                    wsTypes.Cell(1, 1).InsertData(rows);
                }

                var brandsResult = await _mediator.Send(new GetVehiclesBrandsForDisplaying { Skip = 0, Take = MaxRows }, cancellationToken);
                if (brandsResult.IsSuccess && brandsResult.Value.Data.Count > 0)
                {
                    var wsBrands = workbook.AddWorksheet("VehicleBrands");
                    var rows = new List<object[]> { new object[] { "Id", "ArabicName", "EnglishName" } };
                    foreach (var b in brandsResult.Value.Data)
                    {
                        rows.Add(new object[] { b.Id, b.ArabicName ?? "", b.EnglishName ?? "" });
                    }
                    wsBrands.Cell(1, 1).InsertData(rows);
                }

                if (workbook.Worksheets.Count == 0)
                    workbook.AddWorksheet("Empty").Cell(1, 1).Value = "No data";

                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"Vehicles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
