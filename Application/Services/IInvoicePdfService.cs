using Application.Features.CustomerSection.Feature.Order.Dtos;

namespace Application.Services
{
    public interface IInvoicePdfService
    {
        byte[] Generate(OrderInvoiceDto invoice);
    }
}
