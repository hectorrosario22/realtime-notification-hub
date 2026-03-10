using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Events;
using NotificationHub.Domain.Exceptions;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Tests.Entities;

public class PushNotificationTests
{
    [Fact]
    public void Create_WithValidParameters_SetsPropertiesCorrectly()
    {
        var recipientId = Guid.NewGuid();

        var notification = new PushNotification(recipientId, "Title", "Body");

        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(recipientId, notification.RecipientId);
        Assert.Equal("Title", notification.Title);
        Assert.Equal("Body", notification.Body);
        Assert.Equal(NotificationChannel.Push, notification.Channel);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(NotificationPriority.Normal, notification.Priority);
        Assert.True(notification.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(0, notification.RetryCount);
        Assert.Null(notification.ErrorMessage);
    }

    [Fact]
    public void Create_WithEmptyRecipientId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new PushNotification(Guid.Empty, "Title", "Body"));
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new PushNotification(Guid.NewGuid(), "", "Body"));
    }

    [Fact]
    public void Create_WithEmptyBody_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new PushNotification(Guid.NewGuid(), "Title", ""));
    }

    [Fact]
    public void Create_WithCustomPriority_SetsPriority()
    {
        var notification = new PushNotification(
            Guid.NewGuid(), "Title", "Body", NotificationPriority.Critical);

        Assert.Equal(NotificationPriority.Critical, notification.Priority);
    }

    [Fact]
    public void Create_WithMetadata_SetsMetadata()
    {
        var metadata = NotificationMetadata.Create(new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        });

        var notification = new PushNotification(
            Guid.NewGuid(), "Title", "Body", metadata: metadata);

        Assert.Equal("value1", notification.Metadata.Data["key1"]);
        Assert.Equal("value2", notification.Metadata.Data["key2"]);
    }

    [Fact]
    public void Create_RaisesNotificationCreatedEvent()
    {
        var recipientId = Guid.NewGuid();

        var notification = new PushNotification(recipientId, "Title", "Body");

        var domainEvent = Assert.Single(notification.DomainEvents);
        var createdEvent = Assert.IsType<NotificationCreatedEvent>(domainEvent);
        Assert.Equal(notification.Id, createdEvent.NotificationId);
        Assert.Equal(NotificationChannel.Push, createdEvent.Channel);
        Assert.Equal(recipientId, createdEvent.RecipientId);
    }
}
