namespace Application.Features.AdminSection.AuditFeature.Dtos
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public List<AuditLogDetailDto> Details { get; set; } = new();
    }
}
