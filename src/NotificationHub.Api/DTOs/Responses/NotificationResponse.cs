using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.DTOs.Responses;

public record NotificationResponse
{
    public Guid Id { get; init; }
    public NotificationChannel Channel { get; init; }
    public NotificationStatus Status { get; init; }
    public string? RecipientId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? SentAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
    public string? Subject { get; init; }
    public string? HtmlBody { get; init; }
    public string? Content { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, string>? Parameters { get; init; }
    public string? Title { get; init; }
    public DateTime? ReadAt { get; init; }
}
