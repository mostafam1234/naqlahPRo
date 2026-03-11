using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class AuditLog
    {
        private AuditLog()
        {
            ActionName = string.Empty;
            IpAddress = string.Empty;
            UserAgent = string.Empty;
            Details = new List<AuditLogDetail>();
        }

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string ActionName { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        public virtual User User { get; private set; }
        public virtual ICollection<AuditLogDetail> Details { get; private set; }

        public static AuditLog Create(int userId, string actionName, DateTime timestampUtc, string? ipAddress = null, string? userAgent = null)
        {
            return new AuditLog
            {
                UserId = userId,
                ActionName = actionName,
                TimestampUtc = timestampUtc,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Details = new List<AuditLogDetail>()
            };
        }

        public void AddDetail(AuditLogDetail detail)
        {
            if (detail == null)
            {
                return;
            }

            ((List<AuditLogDetail>)Details).Add(detail);
        }
    }
}

