namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class InvoiceLineItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
    }
}
