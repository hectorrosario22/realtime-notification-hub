using Microsoft.AspNetCore.SignalR;

namespace NotificationHub.Api.Hubs;

/// <summary>
/// SignalR hub for real-time push notifications.
/// Clients connect and join a recipient group to receive targeted notifications.
/// </summary>
public class PushNotificationHub : Hub
{
    private const string RecipientIdKey = "RecipientId";

    public override async Task OnConnectedAsync()
    {
        var recipientIdRaw = Context.GetHttpContext()?.Request.Query["recipientId"].ToString();

        if (Guid.TryParse(recipientIdRaw, out var recipientId))
        {
            var groupName = $"recipient-{recipientId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Context.Items[RecipientIdKey] = recipientId;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(RecipientIdKey, out var recipientIdObj) && recipientIdObj is Guid recipientId)
        {
            var groupName = $"recipient-{recipientId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
