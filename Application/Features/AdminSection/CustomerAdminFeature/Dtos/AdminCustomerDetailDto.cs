using Domain.Enums;

namespace Application.Features.AdminSection.CustomerAdminFeature.Dtos
{
    /// <summary>
    /// Full admin view of a customer (account + individual or establishment profile).
    /// </summary>
    public sealed class AdminCustomerDetailDto
    {
        public int CustomerId { get; init; }

        public int UserId { get; init; }

        public CustomerType CustomerType { get; init; }

        public string CustomerTypeName { get; init; } = string.Empty;

        /// <summary>Primary display: trade name (establishment) or identity / phone (individual).</summary>
        public string CustomerDisplayName { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string PhoneNumber { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PlainPasswordExplanation { get; init; }

        public bool HasPasswordConfigured { get; init; }

        public bool IsActive { get; init; }

        public bool IsDeleted { get; init; }

        /// <summary>National address when customer is an establishment.</summary>
        public string? NationalAddress { get; init; }

        public string? TaxRegistrationNumber { get; init; }

        // —— Individual ——
        public string? IndividualIdentityNumber { get; init; }

        public string? IndividualMobileNumber { get; init; }

        public string? IndividualFrontIdentityImagePath { get; init; }

        public string? IndividualBackIdentityImagePath { get; init; }

        // —— Establishment (business) ——
        public string? EstablishmentTradeName { get; init; }

        public string? EstablishmentCommercialRecordImagePath { get; init; }

        public string? EstablishmentTaxRegistrationImagePath { get; init; }

        // —— Establishment representative ——
        public string? EstablishmentRepresentativeName { get; init; }

        public string? EstablishmentRepresentativePhone { get; init; }

        public string? EstablishmentRepresentativeFrontIdentityImagePath { get; init; }

        public string? EstablishmentRepresentativeBackIdentityImagePath { get; init; }

        // —— App devices (domain spelling preserved in storage) ——
        public string AndroidDevice { get; init; } = string.Empty;

        public string IosDevice { get; init; } = string.Empty;
    }
}
