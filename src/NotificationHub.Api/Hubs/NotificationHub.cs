using Microsoft.AspNetCore.SignalR;

namespace NotificationHub.Api.Hubs;

/// <summary>
/// SignalR hub for real-time push notifications.
/// Clients connect and join a recipient group to receive targeted notifications.
/// </summary>
public class PushNotificationHub : Hub
{
    // TODO: On connection, add client to group "recipient-{recipientId}"
    // TODO: On disconnection, remove client from group

    public override async Task OnConnectedAsync()
    {
        // TODO: Extract recipientId from query string or claims and add to group
        // await Groups.AddToGroupAsync(Context.ConnectionId, $"recipient-{recipientId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: Remove from group on disconnect
        await base.OnDisconnectedAsync(exception);
    }
}
