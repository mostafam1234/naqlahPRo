using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.VehicleSection.Dtos;
using Application.Shared.Services;
using CSharpFunctionalExtensions;
using ClosedXML.Excel;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.VehicleSection.Queries
{
    public sealed record ExportVehicleTypeStatisticsToExcelQuery : IRequest<Result<ExportResult>>
    {
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportVehicleTypeStatisticsToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportVehicleTypeStatisticsToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var statsResult = await _mediator.Send(new GetVehicleTypeStatisticsQuery
                {
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    LanguageId = request.LanguageId
                }, cancellationToken);

                if (statsResult.IsFailure)
                    return Result.Failure<ExportResult>(statsResult.Error);

                var stats = statsResult.Value;
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var exportedAt = DateTime.UtcNow;

                var stream = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var summarySheet = workbook.AddWorksheet(isArabic ? "ملخص الإحصائيات" : "StatisticsSummary");
                    var summaryHeader = isArabic
                        ? new object[] { "تاريخ التصدير", "إجمالي أنواع المركبات", "إجمالي المركبات المسجلة" }
                        : new object[] { "ExportDate", "TotalVehicleTypes", "TotalRegisteredVehicles" };

                    var summaryRow = new object[] { exportedAt, stats.TotalVehicleTypes, stats.TotalRegisteredVehicles };
                    summarySheet.Cell(1, 1).InsertData(new List<object[]> { summaryHeader, summaryRow });
                    summarySheet.Column(1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                    var detailsSheet = workbook.AddWorksheet(isArabic ? "تصنيفات الحمولة" : "LoadCategories");
                    var detailsHeader = isArabic
                        ? new object[]
                        {
                            "تصنيف الحمولة",
                            "عدد أنواع المركبات",
                            "عدد المركبات المسجلة"
                        }
                        : new object[]
                        {
                            "LoadCategory",
                            "VehicleTypeCount",
                            "RegisteredVehicleCount"
                        };

                    var detailRows = stats.LoadCategoryCounts
                        .Select(item => new object[]
                        {
                            item.LoadCategoryName,
                            item.VehicleTypeCount,
                            item.RegisteredVehicleCount
                        })
                        .ToList();

                    var allDetailRows = new List<object[]> { detailsHeader };
                    allDetailRows.AddRange(detailRows);
                    detailsSheet.Cell(1, 1).InsertData(allDetailRows);

                    summarySheet.Columns().AdjustToContents();
                    detailsSheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                }

                stream.Position = 0;
                var fileName = $"VehicleTypeStatistics_{exportedAt:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }
        }
    }
}
