using System.Threading;
using System.Threading.Tasks;

namespace Application.Shared.Services
{
    public interface IAuditLogNotifier
    {
        Task NotifyNewAuditLogAsync(CancellationToken cancellationToken = default);
    }
}
