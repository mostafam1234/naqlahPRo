namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class OrderInvoiceDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string? DeliveryManName { get; set; }
        public bool IsProforma { get; set; }
        public decimal TransportAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Total { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<InvoiceLineItemDto> LineItems { get; set; } = new();
    }
}
