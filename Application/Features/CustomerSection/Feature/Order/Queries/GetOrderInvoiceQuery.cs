using Application.Features.CustomerSection.Feature.Order.Dtos;
using Application.Services;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CustomerSection.Feature.Order.Queries
{
    public sealed record GetOrderInvoiceQuery(int OrderId) : IRequest<Result<OrderInvoiceDto>>
    {
        private class Handler : IRequestHandler<GetOrderInvoiceQuery, Result<OrderInvoiceDto>>
        {
            private readonly INaqlahContext _context;
            private readonly IUserSession _userSession;
            private readonly IInvoicePdfService _pdfService;
            private readonly IWebEnvironment _webEnvironment;
            private readonly IReadFromAppSetting _config;

            public Handler(
                INaqlahContext context,
                IUserSession userSession,
                IInvoicePdfService pdfService,
                IWebEnvironment webEnvironment,
                IReadFromAppSetting config)
            {
                _context = context;
                _userSession = userSession;
                _pdfService = pdfService;
                _webEnvironment = webEnvironment;
                _config = config;
            }

            public async Task<Result<OrderInvoiceDto>> Handle(
                GetOrderInvoiceQuery request,
                CancellationToken cancellationToken)
            {
                var languageId = _userSession.LanguageId;

                var customerId = await _context.Customers
                    .Where(c => c.UserId == _userSession.UserId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerId == 0)
                    return Result.Failure<OrderInvoiceDto>("Customer not found.");

                var order = await _context.Orders
                    .Include(o => o.OrderServices)
                    .Include(o => o.OrderStatusHistories)
                    .Where(o => o.Id == request.OrderId && o.CustomerId == customerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (order is null)
                    return Result.Failure<OrderInvoiceDto>("Order not found or access denied.");

                var customerName = await ResolveCustomerNameAsync(customerId, cancellationToken);

                string? deliveryManName = null;
                if (order.DeliveryManId.HasValue)
                {
                    deliveryManName = await _context.DeliveryMen
                        .Where(d => d.Id == order.DeliveryManId.Value)
                        .Select(d => d.FullName)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                // Refunded = wallet credits linked to this order (non-withdrawal transactions)
                var refundedAmount = await _context.WalletTransctions
                    .Where(x => x.OrderId == request.OrderId
                             && x.CustomerId == customerId
                             && !x.Withdraw)
                    .SumAsync(x => x.Amount, cancellationToken);

                var createdDate = order.OrderStatusHistories
                    .OrderBy(h => h.CreationDate)
                    .Select(h => h.CreationDate)
                    .FirstOrDefault();

                var transportAmount = order.Total + order.DiscountAmount;

                var lineItems = order.OrderServices.Select(s => new InvoiceLineItemDto
                {
                    Name = languageId == (int)Language.Arabic ? s.ArabicName : s.EnglishName,
                    Amount = s.Amount,
                    Quantity = 1
                }).ToList();

                var dto = new OrderInvoiceDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = customerName,
                    SenderName = deliveryManName,
                    DeliveryManName = deliveryManName,
                    IsProforma = order.OrderStatus != OrderStatus.Completed,
                    TransportAmount = transportAmount,
                    ServiceFee = 0,
                    TaxAmount = 0,
                    DiscountAmount = order.DiscountAmount,
                    TotalAmount = order.Total,
                    Total = order.Total,
                    RefundedAmount = refundedAmount,
                    CreatedDate = createdDate,
                    LineItems = lineItems
                };

                dto.PdfUrl = GenerateAndSavePdf(dto);

                return Result.Success(dto);
            }

            private string? GenerateAndSavePdf(OrderInvoiceDto invoice)
            {
                try
                {
                    var invoicesFolder = Path.Combine(_webEnvironment.WebRootPath, "Invoices");
                    Directory.CreateDirectory(invoicesFolder);

                    var fileName = $"invoice-{invoice.OrderId}.pdf";
                    var filePath = Path.Combine(invoicesFolder, fileName);

                    var pdfBytes = _pdfService.Generate(invoice);
                    File.WriteAllBytes(filePath, pdfBytes);

                    var baseUrl = _config.GetValue<string>("apiBaseUrl")?.TrimEnd('/');
                    return $"{baseUrl}/Invoices/{fileName}";
                }
                catch
                {
                    return null;
                }
            }

            private async Task<string> ResolveCustomerNameAsync(int customerId, CancellationToken cancellationToken)
            {
                var customer = await _context.Customers
                    .Include(c => c.EstablishMent)
                    .Include(c => c.User)
                    .Where(c => c.Id == customerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (customer is null)
                    return string.Empty;

                if (customer.CustomerType == CustomerType.Establishment && customer.EstablishMent is not null)
                    return customer.EstablishMent.Name;

                return customer.User?.UserName ?? customer.PhoneNumber;
            }
        }
    }
}
