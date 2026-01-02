using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Presentaion.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            System.Console.WriteLine($"[NotificationHub] Client connected: {Context.ConnectionId}");
            System.Console.WriteLine($"[NotificationHub] User: {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            System.Console.WriteLine($"[NotificationHub] Client disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                System.Console.WriteLine($"[NotificationHub] Disconnect error: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}

