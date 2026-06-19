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
    public sealed record ExportDeliveryManSummaryToExcelQuery : IRequest<Result<ExportResult>>
    {
        public int DeliveryManId { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportDeliveryManSummaryToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportDeliveryManSummaryToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var summaryResult = await _mediator.Send(new GetDeliveryManSummaryQuery
                {
                    DeliveryManId = request.DeliveryManId,
                    LanguageId = request.LanguageId
                }, cancellationToken);

                if (summaryResult.IsFailure)
                    return Result.Failure<ExportResult>(summaryResult.Error);

                var summary = summaryResult.Value;
                var isArabic = request.LanguageId == 1;
                var exportedAt = DateTime.UtcNow;

                var header = isArabic
                    ? new object[]
                    {
                        "اسم الكابتن",
                        "تاريخ التصدير",
                        "إجمالي الشحنات",
                        "الشحنات النشطة",
                        "تم تأكيد الذهاب لالتقاط الشحنة",
                        "شحنات تم تسليمها للعميل",
                        "مكتملة",
                        "ملغية"
                    }
                    : new object[]
                    {
                        "CaptainName",
                        "ExportDate",
                        "TotalOrders",
                        "ActiveOrders",
                        "ConfirmedGoingToPickup",
                        "DeliveredToCustomer",
                        "Completed",
                        "Cancelled"
                    };

                var dataRow = new object[]
                {
                    summary.FullName,
                    exportedAt,
                    summary.TotalOrders,
                    summary.ActiveOrders,
                    summary.ConfirmedGoingToPickupOrders,
                    summary.PickedUpOrders,
                    summary.CompletedOrders,
                    summary.CancelledOrders
                };

                var stream = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet(isArabic ? "إحصائيات الشحنات" : "ShipmentStats");
                    ws.Cell(1, 1).InsertData(new List<object[]> { header, dataRow });
                    ws.Column(2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var safeName = string.IsNullOrWhiteSpace(summary.FullName)
                    ? $"DM{summary.Id}"
                    : new string(summary.FullName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray()).Trim();
                if (string.IsNullOrWhiteSpace(safeName))
                    safeName = $"DM{summary.Id}";

                var fileName = $"CaptainShipmentStats_{safeName}_{exportedAt:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
