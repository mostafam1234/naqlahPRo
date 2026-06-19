using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using ClosedXML.Excel;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.OrderFeature.Queries
{
    public sealed record ExportOrderStatisticsToExcelQuery : IRequest<Result<ExportResult>>
    {
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportOrderStatisticsToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportOrderStatisticsToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var statsResult = await _mediator.Send(new GetOrderStatisticsQuery
                {
                    LanguageId = request.LanguageId
                }, cancellationToken);

                if (statsResult.IsFailure)
                    return Result.Failure<ExportResult>(statsResult.Error);

                var stats = statsResult.Value;
                var isArabic = request.LanguageId == 1;
                var exportedAt = DateTime.UtcNow;

                var header = isArabic
                    ? new object[]
                    {
                        "تاريخ التصدير",
                        "إجمالي الطلبات",
                        "الطلبات النشطة",
                        "تم تأكيد الذهاب لالتقاط الشحنة",
                        "شحنات تم تسليمها للعميل",
                        "الطلبات المكتملة",
                        "الطلبات الملغية"
                    }
                    : new object[]
                    {
                        "ExportDate",
                        "TotalOrders",
                        "ActiveOrders",
                        "ConfirmedGoingToPickup",
                        "DeliveredToCustomer",
                        "CompletedOrders",
                        "CancelledOrders"
                    };

                var dataRow = new object[]
                {
                    exportedAt,
                    stats.TotalOrders,
                    stats.ActiveOrders,
                    stats.ConfirmedGoingToPickupOrders,
                    stats.PickedUpOrders,
                    stats.CompletedOrders,
                    stats.CancelledOrders
                };

                var stream = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.AddWorksheet(isArabic ? "إحصائيات الطلبات" : "OrderStats");
                    ws.Cell(1, 1).InsertData(new List<object[]> { header, dataRow });
                    ws.Column(1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var fileName = $"OrderStatistics_{exportedAt:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
