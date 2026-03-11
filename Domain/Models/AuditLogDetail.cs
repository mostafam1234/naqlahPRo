using Domain.Enums;

namespace Domain.Models
{
    public class AuditLogDetail
    {
        private AuditLogDetail()
        {
            EntityType = string.Empty;
            EntityId = string.Empty;
        }

        public int Id { get; private set; }
        public int AuditLogId { get; private set; }
        public string EntityType { get; private set; }
        public string EntityId { get; private set; }
        public AuditChangeType ChangeType { get; private set; }
        public string? OldValuesJson { get; private set; }
        public string? NewValuesJson { get; private set; }

        public virtual AuditLog AuditLog { get; private set; }

        public static AuditLogDetail Create(
            string entityType,
            string entityId,
            AuditChangeType changeType,
            string? oldValuesJson,
            string? newValuesJson)
        {
            return new AuditLogDetail
            {
                EntityType = entityType,
                EntityId = entityId,
                ChangeType = changeType,
                OldValuesJson = oldValuesJson,
                NewValuesJson = newValuesJson
            };
        }
    }
}

