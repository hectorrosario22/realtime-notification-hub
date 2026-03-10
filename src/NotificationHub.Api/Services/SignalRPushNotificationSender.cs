using Microsoft.AspNetCore.SignalR;
using NotificationHub.Api.Hubs;
using NotificationHub.Application.Contracts;
using NotificationHub.Application.Interfaces;

namespace NotificationHub.Api.Services;

public sealed class SignalRPushNotificationSender(IHubContext<PushNotificationHub> hubContext) : IPushNotificationSender
{
    public async Task SendToRecipientAsync(
        Guid recipientId, PushNotificationMessage message, CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"recipient-{recipientId}")
            .SendAsync("ReceiveNotification", message, ct);
    }
}
