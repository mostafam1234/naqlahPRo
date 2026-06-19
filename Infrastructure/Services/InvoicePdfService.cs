using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Services;
using System.Text;

namespace Infrastructure.Services
{
    // Temporary mock implementation (QuestPDF removed): builds a minimal valid
    // PDF by hand so generated invoice files still open in a viewer.
    public class InvoicePdfService : IInvoicePdfService
    {
        public byte[] Generate(OrderInvoiceDto invoice)
        {
            var title = invoice.IsProforma ? "PROFORMA INVOICE (MOCK)" : "INVOICE (MOCK)";

            var lines = new List<string>
            {
                "NAQLAH",
                title,
                $"Order #: {invoice.OrderNumber}",
                $"Customer: {invoice.CustomerName}",
                $"Date: {invoice.CreatedDate:yyyy-MM-dd}",
                string.Empty,
                $"Transport Amount: {invoice.TransportAmount:N2} SAR",
                $"Discount: {invoice.DiscountAmount:N2} SAR",
                $"Total: {invoice.TotalAmount:N2} SAR"
            };

            if (!string.IsNullOrWhiteSpace(invoice.DeliveryManName))
                lines.Insert(4, $"Driver: {invoice.DeliveryManName}");

            if (invoice.RefundedAmount > 0)
                lines.Add($"Refunded: {invoice.RefundedAmount:N2} SAR");

            return BuildSimplePdf(lines);
        }

        private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
        {
            var textOps = new StringBuilder();
            textOps.Append("BT\n/F1 12 Tf\n16 TL\n72 770 Td\n");
            foreach (var line in lines)
                textOps.Append('(').Append(EscapePdfText(line)).Append(") Tj T*\n");
            textOps.Append("ET");

            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
                $"<< /Length {textOps.Length} >>\nstream\n{textOps}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
            };

            // The document is pure ASCII, so char positions equal byte offsets.
            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new int[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                offsets[i] = pdf.Length;
                pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            var xrefOffset = pdf.Length;
            pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets)
                pdf.Append($"{offset:D10} 00000 n \n");
            pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private static string EscapePdfText(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                if (ch is '(' or ')' or '\\')
                    sb.Append('\\');

                // Helvetica with the default encoding can't render non-ASCII
                // (e.g. Arabic names) — substitute until real PDF support returns.
                sb.Append(ch <= 126 && ch >= 32 ? ch : '?');
            }
            return sb.ToString();
        }
    }
}
