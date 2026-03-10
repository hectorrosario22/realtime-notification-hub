using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.Tests.Entities;

public class NotificationRetryTests
{
    private static PushNotification CreateFailedNotification()
    {
        var notification = new PushNotification(Guid.NewGuid(), "Title", "Body");
        notification.MarkAsFailed("Initial failure");
        return notification;
    }

    [Fact]
    public void IncrementRetryCount_IncrementsAndResetsToQueued()
    {
        var notification = CreateFailedNotification();

        notification.IncrementRetryCount();

        Assert.Equal(1, notification.RetryCount);
        Assert.Equal(NotificationStatus.Queued, notification.Status);
        Assert.Null(notification.ErrorMessage);
        Assert.Null(notification.FailedAt);
    }

    [Fact]
    public void IncrementRetryCount_AtMaxRetries_ThrowsDomainException()
    {
        var notification = CreateFailedNotification();

        // Exhaust all retries
        for (var i = 0; i < Notification.MaxRetryCount; i++)
        {
            notification.IncrementRetryCount();
            notification.MarkAsFailed($"Failure {i + 1}");
        }

        Assert.Throws<DomainException>(() =>
            notification.IncrementRetryCount());
    }

    [Fact]
    public void CanRetry_UnderMaxRetries_ReturnsTrue()
    {
        var notification = CreateFailedNotification();

        Assert.True(notification.CanRetry());
    }

    [Fact]
    public void CanRetry_AtMaxRetries_ReturnsFalse()
    {
        var notification = CreateFailedNotification();

        for (var i = 0; i < Notification.MaxRetryCount; i++)
        {
            notification.IncrementRetryCount();
            notification.MarkAsFailed($"Failure {i + 1}");
        }

        Assert.False(notification.CanRetry());
    }
}
