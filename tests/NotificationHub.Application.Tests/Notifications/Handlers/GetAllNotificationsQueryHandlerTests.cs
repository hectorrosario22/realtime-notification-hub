using NotificationHub.Application.Notifications.Handlers;
using NotificationHub.Application.Notifications.Queries;
using NotificationHub.Application.Tests.TestDoubles;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Tests.Notifications.Handlers;

public class GetAllNotificationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMappedNotifications()
    {
        var email = Notification.CreateEmail("user-a", "Subject", "<p>A</p>");
        var push = Notification.CreatePush("user-b", "Title", "Body");

        var repository = new InMemoryNotificationRepository([email, push]);
        var handler = new GetAllNotificationsQueryHandler(repository);

        var result = await handler.Handle(new GetAllNotificationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, n => n.Id == email.Id && n.Subject == "Subject");
        Assert.Contains(result, n => n.Id == push.Id && n.Title == "Title" && n.Content == "Body");
    }
}
