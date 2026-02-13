using NotificationHub.Application.Notifications.Commands;
using NotificationHub.Application.Notifications.Handlers;
using NotificationHub.Application.Tests.TestDoubles;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Tests.Notifications.Handlers;

public class CreateEmailNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateEmailNotificationAndWriteCreatedLog()
    {
        var repository = new InMemoryNotificationRepository();
        var logRepository = new InMemoryNotificationLogRepository();
        var handler = new CreateEmailNotificationCommandHandler(repository, logRepository);

        var command = new CreateEmailNotificationCommand(
            RecipientId: "user-1",
            Subject: "Welcome",
            HtmlBody: "<p>Hello</p>");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(NotificationChannel.Email, result.Channel);
        Assert.Equal(NotificationStatus.Pending, result.Status);
        Assert.Equal("user-1", result.RecipientId);
        Assert.Equal("Welcome", result.Subject);
        Assert.Equal("<p>Hello</p>", result.HtmlBody);

        var logs = await logRepository.GetByNotificationIdAsync(result.Id, CancellationToken.None);
        var createdLog = Assert.Single(logs);
        Assert.Equal(NotificationEventType.Created, createdLog.EventType);
        Assert.Equal("Email notification created", createdLog.Message);
    }
}
