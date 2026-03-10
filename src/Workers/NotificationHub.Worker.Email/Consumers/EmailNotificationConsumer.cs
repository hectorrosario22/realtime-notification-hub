using MassTransit;
using Microsoft.Extensions.Options;
using NotificationHub.Application.Contracts;
using NotificationHub.Worker.Email.Services;

namespace NotificationHub.Worker.Email.Consumers;

public sealed class EmailNotificationConsumer(
    IMailerooEmailSender emailSender,
    IPublishEndpoint publishEndpoint,
    IOptions<MailerooOptions> options,
    ILogger<EmailNotificationConsumer> logger) : IConsumer<EmailNotificationMessage>
{
    public async Task Consume(ConsumeContext<EmailNotificationMessage> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        logger.LogInformation(
            "Processing email notification {NotificationId} to {To}",
            msg.NotificationId, msg.To);

        var mailerooOptions = options.Value;
        var fromAddress = msg.From ?? mailerooOptions.DefaultFromAddress;

        var attachments = msg.AttachmentUrls?.Select(url => new MailerooAttachment(
            FileName: Path.GetFileName(new Uri(url).LocalPath).DefaultIfNullOrEmpty("attachment"),
            ContentType: "application/octet-stream",
            Content: url,
            Inline: false
        )).ToList();

        var request = new MailerooSendRequest(
            From: new MailerooEmailAddress(fromAddress, mailerooOptions.DefaultFromName),
            To: [new MailerooEmailAddress(msg.To)],
            Subject: msg.Subject,
            Html: msg.IsHtml ? msg.Body : null,
            Plain: msg.IsHtml ? null : msg.Body,
            Cc: msg.Cc?.Select(a => new MailerooEmailAddress(a)).ToList(),
            Bcc: msg.Bcc?.Select(a => new MailerooEmailAddress(a)).ToList(),
            ReplyTo: msg.ReplyTo is not null ? new MailerooEmailAddress(msg.ReplyTo) : null,
            Attachments: attachments,
            ReferenceId: GuidExtensions.ToMailerooReferenceId(msg.NotificationId)
        );

        var result = await emailSender.SendAsync(request, ct);

        var statusUpdate = new NotificationStatusUpdate(
            NotificationId: msg.NotificationId,
            RecipientId: msg.RecipientId,
            Channel: "Email",
            Success: result.Success,
            ErrorMessage: result.Success ? null : result.Message,
            ExternalReferenceId: result.ReferenceId,
            Timestamp: DateTime.UtcNow);

        await publishEndpoint.Publish(statusUpdate, ct);

        if (!result.Success)
        {
            logger.LogError(
                "Maileroo failed to send notification {NotificationId}: {Error}",
                msg.NotificationId, result.Message);

            // Throw so MassTransit retries (up to configured retry count)
            throw new InvalidOperationException($"Maileroo send failed: {result.Message}");
        }

        logger.LogInformation(
            "Email notification {NotificationId} sent successfully (ref: {Ref})",
            msg.NotificationId, result.ReferenceId);
    }
}

internal static class StringExtensions
{
    internal static string DefaultIfNullOrEmpty(this string? value, string defaultValue)
        => string.IsNullOrEmpty(value) ? defaultValue : value;
}

file static class GuidExtensions
{
    // Maileroo requires a 24-character hex string for reference_id.
    // Take the first 12 bytes of the Guid (out of 16) → 24 hex chars.
    internal static string ToMailerooReferenceId(Guid id)
        => Convert.ToHexString(id.ToByteArray()[..12]).ToLowerInvariant();
}
