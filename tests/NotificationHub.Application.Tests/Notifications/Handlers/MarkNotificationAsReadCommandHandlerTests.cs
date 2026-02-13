using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Notifications.Commands;
using NotificationHub.Application.Notifications.Handlers;
using NotificationHub.Application.Tests.TestDoubles;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Tests.Notifications.Handlers;

public class MarkNotificationAsReadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotificationDoesNotExist_ShouldReturnNotFoundError()
    {
        var repository = new InMemoryNotificationRepository();
        var handler = new MarkNotificationAsReadCommandHandler(repository);

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(MarkNotificationAsReadError.NotFound, result.Error);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task Handle_WhenNotificationIsNotPush_ShouldReturnInvalidChannelError()
    {
        var email = Notification.CreateEmail("user-1", "Subject", "<p>Body</p>");
        var repository = new InMemoryNotificationRepository([email]);
        var handler = new MarkNotificationAsReadCommandHandler(repository);

        var result = await handler.Handle(new MarkNotificationAsReadCommand(email.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(MarkNotificationAsReadError.InvalidChannel, result.Error);
        Assert.Equal("Only push notifications can be marked as read", result.Message);
    }

    [Fact]
    public async Task Handle_WhenPushStateTransitionIsInvalid_ShouldReturnInvalidStateTransitionError()
    {
        var push = Notification.CreatePush("user-1", "Title", "Body");
        var repository = new InMemoryNotificationRepository([push]);
        var handler = new MarkNotificationAsReadCommandHandler(repository);

        var result = await handler.Handle(new MarkNotificationAsReadCommand(push.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(MarkNotificationAsReadError.InvalidStateTransition, result.Error);
        Assert.Equal("Invalid state transition: Pending -> Read.", result.Message);
    }

    [Fact]
    public async Task Handle_WhenPushIsSent_ShouldMarkAsRead()
    {
        var push = Notification.CreatePush("user-1", "Title", "Body");
        push.MarkAsQueued();
        push.MarkAsSent();

        var repository = new InMemoryNotificationRepository([push]);
        var handler = new MarkNotificationAsReadCommandHandler(repository);

        var result = await handler.Handle(new MarkNotificationAsReadCommand(push.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MarkNotificationAsReadError.None, result.Error);
        Assert.NotNull(result.Notification);
        Assert.Equal(NotificationStatus.Read, result.Notification!.Status);
        Assert.NotNull(result.Notification.ReadAt);
    }
}
