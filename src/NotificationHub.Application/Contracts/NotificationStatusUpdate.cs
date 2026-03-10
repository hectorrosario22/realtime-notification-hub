namespace NotificationHub.Application.Contracts;

public sealed record NotificationStatusUpdate(
    Guid NotificationId,
    Guid RecipientId,
    string Channel,
    bool Success,
    string? ErrorMessage,
    string? ExternalReferenceId,
    DateTime Timestamp);
