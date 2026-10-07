using Microsoft.AspNetCore.SignalR;

namespace RealTimeRailwayTrafficMonitoring.Api.Hubs;

public sealed class TrainHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ConnectionStatus", new
        {
            connected = true,
            message = "Connected to Railway Control Center",
            connectedAtUtc = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
