using Domain.Enums;

namespace Application.Features.AdminSection.AuditFeature.Dtos
{
    public class AuditLogDetailDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public AuditChangeType ChangeType { get; set; }
        public string? OldValuesJson { get; set; }
        public string? NewValuesJson { get; set; }
    }
}
