namespace Application.Features.AdminSection.AuditFeature.Dtos
{
    public class AuditFilterOptionsDto
    {
        public List<string> ActionNames { get; set; } = new();
        public List<string> EntityTypes { get; set; } = new();
    }
}
