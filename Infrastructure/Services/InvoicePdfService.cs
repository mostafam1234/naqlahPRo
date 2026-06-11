using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        private const string ColorPrimary = "#1A6B8A";
        private const string ColorWhite = "#FFFFFF";
        private const string ColorBlack = "#000000";
        private const string ColorGrayLight = "#F5F5F5";
        private const string ColorGrayMed = "#9E9E9E";
        private const string ColorGrayDark = "#616161";
        private const string ColorGreen = "#2E7D32";

        public byte[] Generate(OrderInvoiceDto invoice)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(ColorBlack));

                    page.Header().Element(c => ComposeHeader(c, invoice));
                    page.Content().PaddingVertical(15).Element(c => ComposeContent(c, invoice));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, OrderInvoiceDto invoice)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text("NAQLAH").Bold().FontSize(24).FontColor(ColorPrimary);
                        inner.Item().Text("نقله").FontSize(13).FontColor(ColorGrayMed);
                    });

                    row.RelativeItem().AlignRight().Column(inner =>
                    {
                        var title = invoice.IsProforma ? "PROFORMA INVOICE" : "INVOICE";
                        inner.Item().Text(title).Bold().FontSize(16).FontColor(ColorPrimary);
                        inner.Item().PaddingTop(4).Text($"#{invoice.OrderNumber}").FontSize(11).FontColor(ColorGrayDark);
                    });
                });

                col.Item().PaddingTop(8).LineHorizontal(2).LineColor(ColorPrimary);
            });
        }

        private static void ComposeContent(IContainer container, OrderInvoiceDto invoice)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(15).Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        InfoLine(inner, "Customer", invoice.CustomerName);
                        if (!string.IsNullOrWhiteSpace(invoice.DeliveryManName))
                            InfoLine(inner, "Driver", invoice.DeliveryManName);
                    });

                    row.RelativeItem().AlignRight().Column(inner =>
                    {
                        InfoLineRight(inner, "Date", invoice.CreatedDate.ToString("yyyy-MM-dd"));
                        InfoLineRight(inner, "Status",
                            invoice.IsProforma ? "Proforma (Preliminary)" : "Final Invoice");
                    });
                });

                if (invoice.LineItems.Count > 0)
                    col.Item().PaddingBottom(15).Element(c => ComposeLineItemsTable(c, invoice));

                col.Item().Element(c => ComposeSummary(c, invoice));
            });
        }

        private static void InfoLine(ColumnDescriptor col, string label, string value)
        {
            col.Item().PaddingVertical(2).Text(t =>
            {
                t.Span($"{label}: ").SemiBold().FontColor(ColorGrayDark);
                t.Span(value);
            });
        }

        private static void InfoLineRight(ColumnDescriptor col, string label, string value)
        {
            col.Item().PaddingVertical(2).AlignRight().Text(t =>
            {
                t.Span($"{label}: ").SemiBold().FontColor(ColorGrayDark);
                t.Span(value);
            });
        }

        private static void ComposeLineItemsTable(IContainer container, OrderInvoiceDto invoice)
        {
            container.Column(col =>
            {
                col.Item().Text("Services").Bold().FontSize(11).FontColor(ColorPrimary);

                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);
                        cols.RelativeColumn();
                        cols.ConstantColumn(50);
                        cols.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorPrimary).Padding(5)
                            .Text("#").Bold().FontColor(ColorWhite).FontSize(9);
                        header.Cell().Background(ColorPrimary).Padding(5)
                            .Text("Description").Bold().FontColor(ColorWhite).FontSize(9);
                        header.Cell().Background(ColorPrimary).Padding(5).AlignCenter()
                            .Text("Qty").Bold().FontColor(ColorWhite).FontSize(9);
                        header.Cell().Background(ColorPrimary).Padding(5).AlignRight()
                            .Text("Amount (SAR)").Bold().FontColor(ColorWhite).FontSize(9);
                    });

                    for (var i = 0; i < invoice.LineItems.Count; i++)
                    {
                        var item = invoice.LineItems[i];
                        var bg = i % 2 == 0 ? ColorWhite : ColorGrayLight;

                        table.Cell().Background(bg).Padding(5).Text((i + 1).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(item.Name).FontSize(9);
                        table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text(item.Amount.ToString("N2")).FontSize(9);
                    }
                });
            });
        }

        private static void ComposeSummary(IContainer container, OrderInvoiceDto invoice)
        {
            container.AlignRight().Column(col =>
            {
                col.Item().Text("Summary").Bold().FontSize(11).FontColor(ColorPrimary);

                col.Item().PaddingTop(6).Width(260).Column(inner =>
                {
                    SummaryRow(inner, "Transport Amount", invoice.TransportAmount);

                    if (invoice.ServiceFee != 0)
                        SummaryRow(inner, "Service Fee", invoice.ServiceFee);

                    if (invoice.TaxAmount != 0)
                        SummaryRow(inner, "Tax (VAT)", invoice.TaxAmount);

                    if (invoice.DiscountAmount != 0)
                        SummaryRow(inner, "Discount", -invoice.DiscountAmount, isDiscount: true);

                    inner.Item().PaddingVertical(4).LineHorizontal(1).LineColor(ColorGrayMed);

                    SummaryRow(inner, "TOTAL", invoice.TotalAmount, isBold: true, isHighlighted: true);

                    if (invoice.RefundedAmount > 0)
                        SummaryRow(inner, "Refunded", invoice.RefundedAmount, isDiscount: true);
                });
            });
        }

        private static void SummaryRow(
            ColumnDescriptor col,
            string label,
            decimal amount,
            bool isBold = false,
            bool isHighlighted = false,
            bool isDiscount = false)
        {
            var bg = isHighlighted ? ColorPrimary : ColorWhite;
            var fg = isHighlighted ? ColorWhite : (isDiscount ? ColorGreen : ColorBlack);
            var fontSize = isBold ? 11f : 10f;

            col.Item().Background(bg).PaddingVertical(4).PaddingHorizontal(8).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.DefaultTextStyle(s => isBold ? s.Bold().FontSize(fontSize) : s.FontSize(fontSize));
                    t.Span(label).FontColor(fg);
                });

                row.ConstantItem(90).AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(s => isBold ? s.Bold().FontSize(fontSize) : s.FontSize(fontSize));
                    t.Span($"{amount:N2} SAR").FontColor(fg);
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text("NAQLAH — نقله").FontSize(8).FontColor(ColorGrayMed);

                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("Page ").FontSize(8).FontColor(ColorGrayMed);
                    t.CurrentPageNumber().FontSize(8).FontColor(ColorGrayMed);
                    t.Span(" of ").FontSize(8).FontColor(ColorGrayMed);
                    t.TotalPages().FontSize(8).FontColor(ColorGrayMed);
                });
            });
        }
    }
}
