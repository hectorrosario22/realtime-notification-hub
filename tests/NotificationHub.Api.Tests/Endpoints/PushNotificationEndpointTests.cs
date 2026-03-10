using System.Net;
using System.Net.Http.Json;
using NotificationHub.Api.Tests.Infrastructure;
using NotificationHub.Application.DTOs;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Api.Tests.Endpoints;

public class PushNotificationEndpointTests(NotificationHubWebAppFactory factory)
    : IClassFixture<NotificationHubWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SendPush_WithValidRequest_Returns201Created()
    {
        var request = new SendPushNotificationRequest(
            Guid.NewGuid(), "Test Title", "Test Body");

        var response = await _client.PostAsJsonAsync("/api/notifications/push", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SendPush_WithValidRequest_PersistsNotification()
    {
        var request = new SendPushNotificationRequest(
            Guid.NewGuid(), "Persist Test", "Body content");

        var createResponse = await _client.PostAsJsonAsync("/api/notifications/push", request);
        var created = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/notifications/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("Persist Test", fetched.Title);
        Assert.Equal("Body content", fetched.Body);
        Assert.Equal("Push", fetched.Channel);
        Assert.Equal("Sent", fetched.Status);
    }

    [Fact]
    public async Task SendPush_WithEmptyRecipientId_ThrowsDomainException()
    {
        var request = new SendPushNotificationRequest(
            Guid.Empty, "Title", "Body");

        // Domain validation throws before reaching the HTTP pipeline
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _client.PostAsJsonAsync("/api/notifications/push", request));

        Assert.Contains("RecipientId", ex.Message);
    }

    [Fact]
    public async Task SendPush_WithCustomPriority_SetsPriority()
    {
        var request = new SendPushNotificationRequest(
            Guid.NewGuid(), "Priority Test", "Body", "High");

        var response = await _client.PostAsJsonAsync("/api/notifications/push", request);
        var created = await response.Content.ReadFromJsonAsync<NotificationResponse>();

        Assert.NotNull(created);
        Assert.Equal("High", created.Priority);
    }
}
