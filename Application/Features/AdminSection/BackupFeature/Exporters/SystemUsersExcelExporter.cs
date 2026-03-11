using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.SystemUsers.Queries;
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
    public sealed class SystemUsersExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.SystemUsers;
        private const int MaxRows = 50000;
        private readonly IMediator _mediator;

        public SystemUsersExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllSystemUsersQuery { Skip = 0, Take = MaxRows }, cancellationToken);
            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value.Data;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("SystemUsers");
                var rows = new List<object[]> { new object[] { "Id", "UserName", "Email", "PhoneNumber", "RoleName" } };
                foreach (var u in data)
                {
                    rows.Add(new object[]
                    {
                        u.Id,
                        u.UserName ?? "",
                        u.Email ?? "",
                        u.PhoneNumber ?? "",
                        u.RoleName ?? ""
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"SystemUsers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
