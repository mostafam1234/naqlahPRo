using Application.Shared.Services;
using Microsoft.AspNetCore.SignalR;
using Presentaion.Hubs;
using System.Threading;
using System.Threading.Tasks;

namespace Presentaion.Services
{
    public class AuditLogNotifier : IAuditLogNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuditLogNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewAuditLogAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("NewAuditLog", new { }, cancellationToken);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[AuditLogNotifier] Error sending NewAuditLog: {ex.Message}");
            }
        }
    }
}
