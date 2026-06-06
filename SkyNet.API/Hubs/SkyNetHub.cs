using Microsoft.AspNetCore.SignalR;

namespace SkyNet.API.Hubs;

/// <summary>
/// SignalR Hub for real-time events: queue updates, flight status, emergencies.
/// </summary>
public class SkyNetHub : Hub
{
    public async Task JoinFlightGroup(string flightNumber)
        => await Groups.AddToGroupAsync(Context.ConnectionId, flightNumber);

    public async Task LeaveFlightGroup(string flightNumber)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, flightNumber);

    public async Task SendEmergencyAlert(string airport, string message)
        => await Clients.All.SendAsync("EmergencyAlert", new { airport, message, timestamp = DateTime.UtcNow });

    public async Task BroadcastFlightStatus(string flightNumber, string status)
        => await Clients.All.SendAsync("FlightStatusChanged", new { flightNumber, status, timestamp = DateTime.UtcNow });

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", new { connectionId = Context.ConnectionId, time = DateTime.UtcNow });
        await base.OnConnectedAsync();
    }
}
