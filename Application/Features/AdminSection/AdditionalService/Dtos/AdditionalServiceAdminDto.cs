namespace Application.Features.AdminSection.AdditionalService.Dtos
{
    public class AdditionalServiceAdminDto
    {
        public int Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}
