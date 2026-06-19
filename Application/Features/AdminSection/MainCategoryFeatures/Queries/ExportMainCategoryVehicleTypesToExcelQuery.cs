using Application.Features.AdminSection.BackupFeature.Dtos;
using Application.Features.AdminSection.MainCategoryFeatures.Queries;
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

namespace Application.Features.AdminSection.MainCategoryFeatures.Queries
{
    public sealed record ExportMainCategoryVehicleTypesToExcelQuery : IRequest<Result<ExportResult>>
    {
        public int MainCategoryId { get; init; }
        public int LanguageId { get; init; } = 1;

        private sealed class Handler : IRequestHandler<ExportMainCategoryVehicleTypesToExcelQuery, Result<ExportResult>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<Result<ExportResult>> Handle(
                ExportMainCategoryVehicleTypesToExcelQuery request,
                CancellationToken cancellationToken)
            {
                var dataResult = await _mediator.Send(new GetVehicleTypesByMainCategoryIdQuery
                {
                    MainCategoryId = request.MainCategoryId
                }, cancellationToken);

                if (dataResult.IsFailure)
                {
                    return Result.Failure<ExportResult>(dataResult.Error);
                }

                var data = dataResult.Value;
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var exportedAt = DateTime.UtcNow;

                var stream = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var summarySheet = workbook.AddWorksheet(isArabic ? "ملخص" : "Summary");
                    var summaryHeader = isArabic
                        ? new object[] { "تاريخ التصدير", "صنف الشحن (عربي)", "صنف الشحن (إنجليزي)", "عدد المركبات" }
                        : new object[] { "ExportDate", "ShipmentCategoryArabic", "ShipmentCategoryEnglish", "VehicleCount" };

                    var summaryRow = new object[]
                    {
                        exportedAt,
                        data.MainCategoryArabicName,
                        data.MainCategoryEnglishName,
                        data.VehicleTypes.Count
                    };

                    summarySheet.Cell(1, 1).InsertData(new List<object[]> { summaryHeader, summaryRow });
                    summarySheet.Column(1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                    var detailsSheet = workbook.AddWorksheet(isArabic ? "المركبات المرتبطة" : "LinkedVehicles");
                    var detailsHeader = isArabic
                        ? new object[]
                        {
                            "#",
                            "الاسم بالعربية",
                            "الاسم بالإنجليزية",
                            "تصنيف الحمولة (عربي)",
                            "تصنيف الحمولة (إنجليزي)",
                            "التكلفة"
                        }
                        : new object[]
                        {
                            "#",
                            "ArabicName",
                            "EnglishName",
                            "LoadCategoryArabic",
                            "LoadCategoryEnglish",
                            "Cost"
                        };

                    var detailRows = data.VehicleTypes
                        .Select((item, index) => new object[]
                        {
                            index + 1,
                            item.ArabicName,
                            item.EnglishName,
                            item.LoadCategoryArabicName,
                            item.LoadCategoryEnglishName,
                            item.Cost
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
                var categorySlug = SanitizeFileName(data.MainCategoryEnglishName);
                var fileName = $"MainCategoryVehicles_{categorySlug}_{exportedAt:yyyyMMdd_HHmmss}.xlsx";
                return Result.Success(new ExportResult(stream, fileName));
            }

            private static string SanitizeFileName(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return "Category";
                }

                var invalidChars = Path.GetInvalidFileNameChars();
                var sanitized = new string(value
                    .Where(ch => !invalidChars.Contains(ch))
                    .ToArray())
                    .Replace(' ', '_');

                return string.IsNullOrWhiteSpace(sanitized) ? "Category" : sanitized;
            }
        }
    }
}
