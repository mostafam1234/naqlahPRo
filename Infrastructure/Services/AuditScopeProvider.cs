using System.Threading;
using Domain.InterFaces;

namespace Infrastructure.Services
{
    internal sealed class AuditScopeProvider : IAuditScopeProvider
    {
        private sealed class AuditScope : IAuditScope
        {
            public int UserId { get; init; }
            public string ActionName { get; init; } = string.Empty;
            public string? IpAddress { get; init; }
            public string? UserAgent { get; init; }
        }

        private static readonly AsyncLocal<AuditScope?> Current = new AsyncLocal<AuditScope?>();

        public void SetScope(int userId, string actionName, string? ipAddress = null, string? userAgent = null)
        {
            Current.Value = new AuditScope
            {
                UserId = userId,
                ActionName = actionName,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };
        }

        public IAuditScope? GetCurrentScope()
        {
            return Current.Value;
        }

        public void ClearScope()
        {
            Current.Value = null;
        }
    }
}

