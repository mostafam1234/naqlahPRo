using Application.Features.AdminSection.WalletTransactionFeatures.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.WalletTransactionFeatures.Queries
{
    public sealed record GetCustomersLookupQuery : IRequest<Result<List<CustomerLookupDto>>>
    {
        public string? SearchTerm { get; init; }

        private class GetCustomersLookupQueryHandler : IRequestHandler<GetCustomersLookupQuery, Result<List<CustomerLookupDto>>>
        {
            private readonly INaqlahContext _context;
            public GetCustomersLookupQueryHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<Result<List<CustomerLookupDto>>> Handle(GetCustomersLookupQuery request, CancellationToken cancellationToken)
            {
                var query = from c in _context.Customers
                           join u in _context.Users on c.UserId equals u.Id
                           select new
                           {
                               Customer = c,
                               UserName = u.UserName ?? "غير محدد",
                               PhoneNumber = c.PhoneNumber
                           };

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.PhoneNumber.Contains(request.SearchTerm) ||
                                           x.UserName.Contains(request.SearchTerm));
                }

                var customers = await query
                    .Select(x => new CustomerLookupDto
                    {
                        Id = x.Customer.Id,
                        Name = x.UserName,
                        PhoneNumber = x.PhoneNumber
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync(cancellationToken);

                return Result.Success(customers);
            }
        }
    }
}

