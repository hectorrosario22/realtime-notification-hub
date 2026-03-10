namespace NotificationHub.Application.DTOs;

public sealed record NotificationResponse(
    Guid Id,
    Guid RecipientId,
    string Title,
    string Body,
    string Channel,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? FailedAt,
    string? ErrorMessage);
