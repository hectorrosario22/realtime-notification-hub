using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Events;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.Tests.Entities;

public class NotificationStateTests
{
    private static PushNotification CreateNotification() =>
        new(Guid.NewGuid(), "Title", "Body");

    [Fact]
    public void MarkAsSent_FromPending_SetsStatusAndTimestamp()
    {
        var notification = CreateNotification();

        notification.MarkAsSent();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
    }

    [Fact]
    public void MarkAsSent_FromQueued_SetsStatusAndTimestamp()
    {
        var notification = CreateNotification();
        notification.MarkAsQueued();

        notification.MarkAsSent();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
    }

    [Fact]
    public void MarkAsSent_FromDelivered_ThrowsInvalidStateException()
    {
        var notification = CreateNotification();
        notification.MarkAsSent();
        notification.MarkAsDelivered();

        Assert.Throws<InvalidNotificationStateException>(() =>
            notification.MarkAsSent());
    }

    [Fact]
    public void MarkAsSent_RaisesNotificationSentEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents(); // clear NotificationCreatedEvent

        notification.MarkAsSent();

        var domainEvent = Assert.Single(notification.DomainEvents);
        var sentEvent = Assert.IsType<NotificationSentEvent>(domainEvent);
        Assert.Equal(notification.Id, sentEvent.NotificationId);
    }

    [Fact]
    public void MarkAsDelivered_FromSent_SetsStatusAndTimestamp()
    {
        var notification = CreateNotification();
        notification.MarkAsSent();

        notification.MarkAsDelivered();

        Assert.Equal(NotificationStatus.Delivered, notification.Status);
        Assert.NotNull(notification.DeliveredAt);
    }

    [Fact]
    public void MarkAsDelivered_FromPending_ThrowsInvalidStateException()
    {
        var notification = CreateNotification();

        Assert.Throws<InvalidNotificationStateException>(() =>
            notification.MarkAsDelivered());
    }

    [Fact]
    public void MarkAsFailed_SetsStatusAndErrorMessage()
    {
        var notification = CreateNotification();

        notification.MarkAsFailed("Connection timeout");

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal("Connection timeout", notification.ErrorMessage);
        Assert.NotNull(notification.FailedAt);
    }

    [Fact]
    public void MarkAsFailed_WithEmptyReason_ThrowsDomainException()
    {
        var notification = CreateNotification();

        Assert.Throws<DomainException>(() =>
            notification.MarkAsFailed(""));
    }

    [Fact]
    public void MarkAsFailed_RaisesNotificationFailedEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.MarkAsFailed("Error occurred");

        var domainEvent = Assert.Single(notification.DomainEvents);
        var failedEvent = Assert.IsType<NotificationFailedEvent>(domainEvent);
        Assert.Equal(notification.Id, failedEvent.NotificationId);
        Assert.Equal("Error occurred", failedEvent.Reason);
    }

    [Fact]
    public void MarkAsQueued_FromPending_SetsStatusAndTimestamp()
    {
        var notification = CreateNotification();

        notification.MarkAsQueued();

        Assert.Equal(NotificationStatus.Queued, notification.Status);
        Assert.NotNull(notification.QueuedAt);
    }

    [Fact]
    public void MarkAsQueued_FromSent_ThrowsInvalidStateException()
    {
        var notification = CreateNotification();
        notification.MarkAsSent();

        Assert.Throws<InvalidNotificationStateException>(() =>
            notification.MarkAsQueued());
    }

    [Fact]
    public void Cancel_FromPending_SetsCancelledStatus()
    {
        var notification = CreateNotification();

        notification.Cancel();

        Assert.Equal(NotificationStatus.Cancelled, notification.Status);
    }

    [Fact]
    public void Cancel_FromSent_ThrowsInvalidStateException()
    {
        var notification = CreateNotification();
        notification.MarkAsSent();

        Assert.Throws<InvalidNotificationStateException>(() =>
            notification.Cancel());
    }
}
