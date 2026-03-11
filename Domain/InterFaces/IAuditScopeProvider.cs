using System;

namespace Domain.InterFaces
{
    public interface IAuditScope
    {
        int UserId { get; }
        string ActionName { get; }
        string? IpAddress { get; }
        string? UserAgent { get; }
    }

    public interface IAuditScopeProvider
    {
        void SetScope(int userId, string actionName, string? ipAddress = null, string? userAgent = null);
        IAuditScope? GetCurrentScope();
        void ClearScope();
    }
}

