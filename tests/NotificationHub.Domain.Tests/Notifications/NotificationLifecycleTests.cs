using NotificationHub.Domain.Notifications;

namespace NotificationHub.Domain.Tests.Notifications;

public class NotificationLifecycleTests
{
    [Fact]
    public void CreateEmail_ShouldStartAsPending()
    {
        var notification = Notification.CreateEmail(
            recipientId: "user-1",
            subject: "Subject",
            htmlBody: "<p>Body</p>");

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(NotificationChannel.Email, notification.Channel);
        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.NotEqual(default, notification.CreatedAtUtc);
    }

    [Fact]
    public void MarkAsQueued_FromPending_ShouldTransitionToQueued()
    {
        var notification = CreateEmailNotification();

        notification.MarkAsQueued();

        Assert.Equal(NotificationStatus.Queued, notification.Status);
        Assert.NotNull(notification.UpdatedAtUtc);
    }

    [Fact]
    public void MarkAsQueued_FromFailedWithRetriesAvailable_ShouldTransitionToQueued()
    {
        var notification = CreateEmailNotification(maxRetries: 2);
        notification.MarkAsQueued();
        notification.MarkAsFailed("provider timeout");

        notification.MarkAsQueued();

        Assert.Equal(NotificationStatus.Queued, notification.Status);
        Assert.Equal(1, notification.RetryCount);
        Assert.Null(notification.ErrorMessage);
    }

    [Fact]
    public void MarkAsQueued_FromFailedWithoutRetriesAvailable_ShouldThrow()
    {
        var notification = CreateEmailNotification(maxRetries: 1);
        notification.MarkAsQueued();
        notification.MarkAsFailed("provider timeout");

        var act = () => notification.MarkAsQueued();

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Cannot queue notification: maximum retries reached.", ex.Message);
    }

    [Fact]
    public void StartProcessing_FromQueued_ShouldTransitionToProcessing()
    {
        var notification = CreateEmailNotification();
        notification.MarkAsQueued();

        notification.StartProcessing();

        Assert.Equal(NotificationStatus.Processing, notification.Status);
    }

    [Fact]
    public void StartProcessing_FromPending_ShouldThrow()
    {
        var notification = CreateEmailNotification();

        var act = () => notification.StartProcessing();

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid state transition: Pending -> Processing.", ex.Message);
    }

    [Fact]
    public void MarkAsSent_FromQueued_ShouldTransitionToSentAndSetTimestamp()
    {
        var notification = CreateEmailNotification();
        notification.MarkAsQueued();

        notification.MarkAsSent();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAtUtc);
        Assert.Null(notification.ErrorMessage);
    }

    [Fact]
    public void MarkAsSent_FromPending_ShouldThrow()
    {
        var notification = CreateEmailNotification();

        var act = () => notification.MarkAsSent();

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid state transition: Pending -> Sent.", ex.Message);
    }

    [Fact]
    public void MarkAsFailed_FromProcessing_ShouldIncrementRetryAndSetError()
    {
        var notification = CreateEmailNotification();
        notification.MarkAsQueued();
        notification.StartProcessing();

        notification.MarkAsFailed("provider unavailable");

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(1, notification.RetryCount);
        Assert.Equal("provider unavailable", notification.ErrorMessage);
    }

    [Fact]
    public void MarkAsRead_FromSent_ShouldTransitionToReadAndSetTimestamp()
    {
        var notification = CreateEmailNotification();
        notification.MarkAsQueued();
        notification.MarkAsSent();

        notification.MarkAsRead();

        Assert.Equal(NotificationStatus.Read, notification.Status);
        Assert.NotNull(notification.ReadAtUtc);
    }

    [Fact]
    public void MarkAsRead_FromFailed_ShouldThrow()
    {
        var notification = CreateEmailNotification();
        notification.MarkAsQueued();
        notification.MarkAsFailed("provider timeout");

        var act = () => notification.MarkAsRead();

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid state transition: Failed -> Read.", ex.Message);
    }

    private static Notification CreateEmailNotification(int maxRetries = 3)
    {
        return Notification.CreateEmail(
            recipientId: "user-1",
            subject: "Subject",
            htmlBody: "<p>Body</p>",
            maxRetries: maxRetries);
    }
}
