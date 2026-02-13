using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Common.Models;

public sealed record NotificationResult(
    Guid Id,
    NotificationChannel Channel,
    NotificationStatus Status,
    string RecipientId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? SentAt,
    string? ErrorMessage,
    int RetryCount,
    string? Subject,
    string? HtmlBody,
    string? Content,
    string? TemplateName,
    IReadOnlyDictionary<string, string>? Parameters,
    string? Title,
    DateTime? ReadAt);
