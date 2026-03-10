namespace NotificationHub.Application.DTOs;

public sealed record SendPushNotificationRequest(
    Guid RecipientId,
    string Title,
    string Body,
    string Priority = "Normal",
    Dictionary<string, string>? Metadata = null);
