using Domain.Enums;

namespace Application.Features.CustomerSection.Feature.CustomerInfo.Dtos
{
    public class CustomerInfoDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public CustomerType CustomerType { get; set; }
        public decimal WalletBalance { get; set; }
        public IndividualDto? Individual { get; set; }
        public EstablishmentDto? Establishment { get; set; }
    }

    public class IndividualDto
    {
        public int Id { get; set; }
        public string MobileNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string FrontIdentityImagePath { get; set; }
        public string BackIdentityImagePath { get; set; }
    }

    public class EstablishmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string RecordImagePath { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string TaxRegistrationImagePath { get; set; }
        public string Address { get; set; }
        public EstablishmentRepresentativeDto Representative { get; set; }
    }

    public class EstablishmentRepresentativeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string FrontIdentityNumberImagePath { get; set; }
        public string BackIdentityNumberImagePath { get; set; }
    }
}