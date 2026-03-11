using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.BackupFeature.Queries;
using Application.Features.AdminSection.WalletTransactionFeatures.Queries;
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
    public sealed class WalletTransactionsExcelExporter : IModuleExporter
    {
        public string ModuleKey => BackupModuleKeys.WalletTransactions;
        private readonly IMediator _mediator;

        public WalletTransactionsExcelExporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ExportResult>> ExportAsync(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            var queryResult = await _mediator.Send(new GetAllWalletTransactionsForExportQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }, cancellationToken);
            if (queryResult.IsFailure)
                return Result.Failure<ExportResult>(queryResult.Error);

            var data = queryResult.Value;
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("WalletTransactions");
                var rows = new List<object[]>
                {
                    new object[] { "Id", "ArabicDescription", "EnglishDescription", "Amount", "Withdraw", "OrderId", "CustomerId", "CustomerName", "CustomerPhoneNumber", "CreatedDate" }
                };
                foreach (var t in data)
                {
                    rows.Add(new object[]
                    {
                        t.Id,
                        t.ArabicDescription ?? "",
                        t.EnglishDescription ?? "",
                        t.Amount,
                        t.Withdraw,
                        t.OrderId?.ToString() ?? "",
                        t.CustomerId,
                        t.CustomerName ?? "",
                        t.CustomerPhoneNumber ?? "",
                        t.CreatedDate
                    });
                }
                ws.Cell(1, 1).InsertData(rows);
                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            var fileName = $"WalletTransactions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return Result.Success(new ExportResult(stream, fileName));
        }
    }
}
