using Application.Features.AdminSection.CustomerAdminFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.CustomerAdminFeature.Queries
{
    public sealed record GetCustomerAdminByIdQuery : IRequest<Result<AdminCustomerDetailDto>>
    {
        public int CustomerId { get; init; }

        public int LanguageId { get; init; } = 1;

        private class GetCustomerAdminByIdQueryHandler : IRequestHandler<GetCustomerAdminByIdQuery, Result<AdminCustomerDetailDto>>
        {
            private const string PasswordExplanationAr =
                "كلمة المرور غير المعروضة لأنها مخزنة بشكل مشفّر؛ يمكنك إعادة تعيين كلمة المرور من لوحة الإدارة.";

            private const string PasswordExplanationEn =
                "Plain password cannot be shown (hashed storage). Use reset password to set a new one.";

            private const string CustomerFolderPrefix = "Customer";

            private readonly INaqlahContext _context;
            private readonly IReadFromAppSetting _config;

            public GetCustomerAdminByIdQueryHandler(INaqlahContext context, IReadFromAppSetting config)
            {
                _context = context;
                _config = config;
            }

            public async Task<Result<AdminCustomerDetailDto>> Handle(GetCustomerAdminByIdQuery request, CancellationToken cancellationToken)
            {
                if (request.CustomerId <= 0)
                {
                    return Result.Failure<AdminCustomerDetailDto>("CustomerIdInvalid");
                }

                var isArabic = request.LanguageId == (int)Language.Arabic;
                var pwdNote = isArabic ? PasswordExplanationAr : PasswordExplanationEn;
                var baseUrl = _config.GetValue<string>("apiBaseUrl")?.TrimEnd('/') ?? string.Empty;

                var dto = await (
                    from c in _context.Customers.AsNoTracking()
                    join u in _context.Users.AsNoTracking() on c.UserId equals u.Id
                    where c.Id == request.CustomerId
                    select new AdminCustomerDetailDto
                    {
                        CustomerId = c.Id,
                        UserId = u.Id,
                        CustomerType = c.CustomerType,
                        CustomerTypeName = c.CustomerType == CustomerType.Establishment
                            ? (isArabic ? "شركة / مؤسسة" : "Establishment")
                            : (isArabic ? "فرد" : "Individual"),
                        CustomerDisplayName = c.CustomerType == CustomerType.Establishment && c.EstablishMent != null
                            ? c.EstablishMent.Name
                            : ((c.Individual != null
                                ? (string.IsNullOrWhiteSpace(c.Individual.IdentityNumber)
                                    ? c.PhoneNumber
                                    : c.Individual.IdentityNumber)
                                : c.PhoneNumber) ?? string.Empty),
                        UserName = u.UserName ?? string.Empty,
                        PhoneNumber = c.PhoneNumber,
                        Email = u.Email ?? string.Empty,
                        PlainPasswordExplanation = pwdNote,
                        HasPasswordConfigured = u.PasswordHash != null && u.PasswordHash != string.Empty,
                        NationalAddress = c.CustomerType == CustomerType.Establishment && c.EstablishMent != null
                            ? c.EstablishMent.Address
                            : null,
                        TaxRegistrationNumber = c.CustomerType == CustomerType.Establishment && c.EstablishMent != null
                            ? c.EstablishMent.TaxRegistrationNumber
                            : null,
                        IsActive = u.IsActive,
                        IsDeleted = u.IsDeleted,
                        IndividualIdentityNumber = c.Individual != null ? c.Individual.IdentityNumber : null,
                        IndividualMobileNumber = c.Individual != null ? c.Individual.MobileNumber : null,
                        IndividualFrontIdentityImagePath = c.Individual != null ? c.Individual.FrontIdentityImagePath : null,
                        IndividualBackIdentityImagePath = c.Individual != null ? c.Individual.BackIdentityImagePath : null,
                        EstablishmentTradeName = c.EstablishMent != null ? c.EstablishMent.Name : null,
                        EstablishmentCommercialRecordImagePath = c.EstablishMent != null ? c.EstablishMent.RecoredImagePath : null,
                        EstablishmentTaxRegistrationImagePath = c.EstablishMent != null ? c.EstablishMent.TaxRegistrationImagePath : null,
                        EstablishmentRepresentativeName = c.EstablishMent != null && c.EstablishMent.EstablishMentRepresentitive != null
                            ? c.EstablishMent.EstablishMentRepresentitive.Name
                            : null,
                        EstablishmentRepresentativePhone = c.EstablishMent != null && c.EstablishMent.EstablishMentRepresentitive != null
                            ? c.EstablishMent.EstablishMentRepresentitive.PhoneNumber
                            : null,
                        EstablishmentRepresentativeFrontIdentityImagePath =
                            c.EstablishMent != null && c.EstablishMent.EstablishMentRepresentitive != null
                                ? c.EstablishMent.EstablishMentRepresentitive.FrontIdentityNumberImagePath
                                : null,
                        EstablishmentRepresentativeBackIdentityImagePath =
                            c.EstablishMent != null && c.EstablishMent.EstablishMentRepresentitive != null
                                ? c.EstablishMent.EstablishMentRepresentitive.BackIdentityNumberImagePath
                                : null,
                        AndroidDevice = c.AndriodDevice ?? string.Empty,
                        IosDevice = c.IosDevice ?? string.Empty
                    }).FirstOrDefaultAsync(cancellationToken);

                if (dto == null)
                {
                    return Result.Failure<AdminCustomerDetailDto>("CustomerNotFound");
                }

                var roleOk = await _context.UserRoles
                    .AsNoTracking()
                    .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == Role.Customer.Id, cancellationToken);

                if (!roleOk)
                {
                    return Result.Failure<AdminCustomerDetailDto>("RecordNotCustomerAccount");
                }

                dto.IndividualFrontIdentityImagePath = BuildImageUrl(dto.IndividualFrontIdentityImagePath, baseUrl);
                dto.IndividualBackIdentityImagePath = BuildImageUrl(dto.IndividualBackIdentityImagePath, baseUrl);
                dto.EstablishmentCommercialRecordImagePath = BuildImageUrl(dto.EstablishmentCommercialRecordImagePath, baseUrl);
                dto.EstablishmentTaxRegistrationImagePath = BuildImageUrl(dto.EstablishmentTaxRegistrationImagePath, baseUrl);
                dto.EstablishmentRepresentativeFrontIdentityImagePath =
                    BuildImageUrl(dto.EstablishmentRepresentativeFrontIdentityImagePath, baseUrl);
                dto.EstablishmentRepresentativeBackIdentityImagePath =
                    BuildImageUrl(dto.EstablishmentRepresentativeBackIdentityImagePath, baseUrl);

                return Result.Success(dto);
            }

            private static string? BuildImageUrl(string? path, string baseUrl)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                var trimmed = path.Trim();
                if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed;
                }

                if (trimmed.Contains("/ImageBank/", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.StartsWith('/')
                        ? $"{baseUrl}{trimmed}"
                        : trimmed;
                }

                var fileName = trimmed.Replace('\\', '/').Split('/').Last();
                return $"{baseUrl}/ImageBank/{CustomerFolderPrefix}/{fileName}";
            }
        }
    }
}
