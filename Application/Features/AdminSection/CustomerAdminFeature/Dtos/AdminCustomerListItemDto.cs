using Domain.Enums;

namespace Application.Features.AdminSection.CustomerAdminFeature.Dtos
{
    public sealed class AdminCustomerListItemDto
    {
        public int CustomerId { get; init; }

        /// <summary>Linked AspNet Identity user Id.</summary>
        public int UserId { get; init; }

        public CustomerType CustomerType { get; init; }

        public string CustomerTypeName { get; init; } = string.Empty;

        /// <summary>Establishment trade name or individual identity identifier / phone fallback.</summary>
        public string CustomerDisplayName { get; init; } = string.Empty;

        /// <summary>Login name (typically phone).</summary>
        public string UserName { get; init; } = string.Empty;

        public string PhoneNumber { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        /// <summary>Plain password cannot be retrieved; explanation for the admin UI.</summary>
        public string? PlainPasswordExplanation { get; init; }

        /// <summary>Whether a password hash exists on the identity user.</summary>
        public bool HasPasswordConfigured { get; init; }

        /// <summary>Establishment national address only.</summary>
        public string? NationalAddress { get; init; }

        /// <summary>Tax / registration number for establishment.</summary>
        public string? TaxRegistrationNumber { get; init; }

        public bool IsActive { get; init; }

        public bool IsDeleted { get; init; }
    }
}
