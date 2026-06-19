using Application.Features.AdminSection.CustomerAdminFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CustomerAdminFeature.Queries
{
    public sealed record GetAllCustomersAdminQuery : IRequest<Result<PagedResult<AdminCustomerListItemDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public int LanguageId { get; init; } = 1;

        private class GetAllCustomersAdminQueryHandler : IRequestHandler<GetAllCustomersAdminQuery, Result<PagedResult<AdminCustomerListItemDto>>>
        {
            private const string PasswordExplanationAr =
                "كلمة المرور غير معروضة لأنها مخزنة بشكل مشفّر؛ يمكنك إعادة تعيين كلمة المرور من لوحة الإدارة.";

            private const string PasswordExplanationEn =
                "Plain password cannot be shown (hashed storage). Use reset password to set a new one.";

            private readonly INaqlahContext _context;

            public GetAllCustomersAdminQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<AdminCustomerListItemDto>>> Handle(GetAllCustomersAdminQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var pwdNote = isArabic ? PasswordExplanationAr : PasswordExplanationEn;

                var query =
                    from c in _context.Customers.AsNoTracking()
                    join u in _context.Users.AsNoTracking() on c.UserId equals u.Id
                    select new { c, u };

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var term = request.SearchTerm.Trim().ToLower();
                    query = query.Where(x =>
                        x.c.PhoneNumber.ToLower().Contains(term) ||
                        (x.u.Email != null && x.u.Email.ToLower().Contains(term)) ||
                        (x.u.UserName != null && x.u.UserName.ToLower().Contains(term)) ||
                        (x.c.EstablishMent != null && (
                            x.c.EstablishMent.Name.ToLower().Contains(term) ||
                            x.c.EstablishMent.Address.ToLower().Contains(term) ||
                            (x.c.EstablishMent.TaxRegistrationNumber ?? string.Empty).ToLower().Contains(term))) ||
                        (x.c.Individual != null &&
                            (x.c.Individual.IdentityNumber ?? string.Empty).ToLower().Contains(term)));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var page = await query
                    .OrderByDescending(x => x.c.Id)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new AdminCustomerListItemDto
                    {
                        CustomerId = x.c.Id,
                        UserId = x.u.Id,
                        CustomerType = x.c.CustomerType,
                        CustomerTypeName = x.c.CustomerType == CustomerType.Establishment
                            ? (isArabic ? "شركة / مؤسسة" : "Establishment")
                            : (isArabic ? "فرد" : "Individual"),
                        CustomerDisplayName = x.c.CustomerType == CustomerType.Establishment && x.c.EstablishMent != null
                            ? x.c.EstablishMent.Name
                            : ((x.c.Individual != null
                                ? (string.IsNullOrWhiteSpace(x.c.Individual.IdentityNumber)
                                    ? x.c.PhoneNumber
                                    : x.c.Individual.IdentityNumber)
                                : x.c.PhoneNumber) ?? string.Empty),
                        UserName = x.u.UserName ?? string.Empty,
                        PhoneNumber = x.c.PhoneNumber,
                        Email = x.u.Email ?? string.Empty,
                        PlainPasswordExplanation = pwdNote,
                        HasPasswordConfigured = x.u.PasswordHash != null && x.u.PasswordHash != string.Empty,
                        NationalAddress = x.c.CustomerType == CustomerType.Establishment && x.c.EstablishMent != null
                            ? x.c.EstablishMent.Address
                            : null,
                        TaxRegistrationNumber = x.c.CustomerType == CustomerType.Establishment && x.c.EstablishMent != null
                            ? x.c.EstablishMent.TaxRegistrationNumber
                            : null,
                        IsActive = x.u.IsActive,
                        IsDeleted = x.u.IsDeleted
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.Take);
                return Result.Success(new PagedResult<AdminCustomerListItemDto>
                {
                    Data = page,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                });
            }
        }
    }
}
