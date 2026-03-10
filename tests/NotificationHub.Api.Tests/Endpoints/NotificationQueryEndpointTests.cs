using System.Net;
using System.Net.Http.Json;
using NotificationHub.Api.Tests.Infrastructure;
using NotificationHub.Application.DTOs;

namespace NotificationHub.Api.Tests.Endpoints;

public class NotificationQueryEndpointTests(NotificationHubWebAppFactory factory)
    : IClassFixture<NotificationHubWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetById_ExistingNotification_Returns200WithData()
    {
        // Create a notification first
        var request = new SendPushNotificationRequest(
            Guid.NewGuid(), "Query Test", "Body");
        var createResponse = await _client.PostAsJsonAsync("/api/notifications/push", request);
        var created = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(created);

        // Query by ID
        var response = await _client.GetAsync($"/api/notifications/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Query Test", result.Title);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/notifications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFailed_ReturnsOnlyFailedNotifications()
    {
        // Send a valid push (will be Sent status, not Failed)
        var request = new SendPushNotificationRequest(
            Guid.NewGuid(), "Not Failed", "Body");
        await _client.PostAsJsonAsync("/api/notifications/push", request);

        var response = await _client.GetAsync("/api/notifications/failed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        Assert.NotNull(results);
        Assert.DoesNotContain(results, n => n.Title == "Not Failed");
    }
}
