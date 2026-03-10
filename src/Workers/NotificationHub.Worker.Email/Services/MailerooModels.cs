using System.Text.Json.Serialization;

namespace NotificationHub.Worker.Email.Services;

// --- Request models ---

public sealed record MailerooEmailAddress(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("display_name")] string? DisplayName = null);

public sealed record MailerooAttachment(
    [property: JsonPropertyName("file_name")] string FileName,
    [property: JsonPropertyName("content_type")] string ContentType,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("inline")] bool Inline = false);

public sealed record MailerooSendRequest(
    [property: JsonPropertyName("from")] MailerooEmailAddress From,
    [property: JsonPropertyName("to")] List<MailerooEmailAddress> To,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("html")] string? Html,
    [property: JsonPropertyName("plain")] string? Plain,
    [property: JsonPropertyName("cc")] List<MailerooEmailAddress>? Cc,
    [property: JsonPropertyName("bcc")] List<MailerooEmailAddress>? Bcc,
    [property: JsonPropertyName("reply_to")] MailerooEmailAddress? ReplyTo,
    [property: JsonPropertyName("attachments")] List<MailerooAttachment>? Attachments,
    [property: JsonPropertyName("reference_id")] string? ReferenceId,
    [property: JsonPropertyName("tracking")] bool Tracking = true);

// --- Response models ---

public sealed record MailerooSendResponseData(
    [property: JsonPropertyName("reference_id")] string? ReferenceId);

public sealed record MailerooSendResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("data")] MailerooSendResponseData? Data);

// --- Result ---

public sealed record MailerooSendResult(bool Success, string? Message, string? ReferenceId);
