using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using NotificationHub.Api.Tests.Infrastructure;
using NotificationHub.Application.DTOs;

namespace NotificationHub.Api.Tests.Hubs;

public class PushNotificationHubTests(NotificationHubWebAppFactory factory)
    : IClassFixture<NotificationHubWebAppFactory>
{
    [Fact]
    public async Task SendPush_ClientConnected_ReceivesNotificationViaSignalR()
    {
        var recipientId = Guid.NewGuid();
        var client = factory.CreateClient();
        var serverUrl = client.BaseAddress!.ToString().TrimEnd('/');

        await using var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hubs/notifications?recipientId={recipientId}",
                options => options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
            .Build();

        var receivedTcs = new TaskCompletionSource<NotificationReceivedPayload>();

        hubConnection.On<NotificationReceivedPayload>("ReceiveNotification", payload =>
        {
            receivedTcs.TrySetResult(payload);
        });

        await hubConnection.StartAsync();

        // Send a push notification targeting this recipient
        var request = new SendPushNotificationRequest(
            recipientId, "SignalR Test", "Real-time body");

        await client.PostAsJsonAsync("/api/notifications/push", request);

        // Wait for the SignalR message (with timeout)
        var received = await Task.WhenAny(receivedTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.Equal(receivedTcs.Task, received);
        var payload = await receivedTcs.Task;
        Assert.Equal("SignalR Test", payload.Title);
        Assert.Equal("Real-time body", payload.Body);

        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task Hub_ConnectWithoutRecipientId_DoesNotThrow()
    {
        var client = factory.CreateClient();
        var serverUrl = client.BaseAddress!.ToString().TrimEnd('/');

        await using var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hubs/notifications",
                options => options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
            .Build();

        // Should connect without error even without recipientId
        await hubConnection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);

        await hubConnection.StopAsync();
    }

    // Simple record to deserialize SignalR message
    private sealed record NotificationReceivedPayload(
        Guid NotificationId,
        string Title,
        string Body,
        string Priority,
        Dictionary<string, string> Metadata,
        DateTime SentAt);
}
