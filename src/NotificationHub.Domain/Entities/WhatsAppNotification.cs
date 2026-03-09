using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Entities;

public sealed class WhatsAppNotification : Notification
{
    public PhoneNumber PhoneNumber { get; private init; } = default!;
    public string? TemplateId { get; private init; }
    public string? TemplateName { get; private init; }

    /// <summary>Parámetros dinámicos para los placeholders del header del template ({{1}}, {{2}}, etc.)</summary>
    public List<string> HeaderParameters { get; private init; } = [];

    /// <summary>Parámetros dinámicos para los placeholders del body del template ({{1}}, {{2}}, etc.)</summary>
    public List<string> BodyParameters { get; private init; } = [];

    /// <summary>Parámetros dinámicos para los placeholders del footer del template ({{1}}, {{2}}, etc.)</summary>
    public List<string> FooterParameters { get; private init; } = [];

    // EF Core
    private WhatsAppNotification() { }

    public WhatsAppNotification(
        Guid recipientId,
        string title,
        string body,
        PhoneNumber phoneNumber,
        NotificationPriority priority = NotificationPriority.Normal,
        string? templateId = null,
        string? templateName = null,
        IEnumerable<string>? headerParameters = null,
        IEnumerable<string>? bodyParameters = null,
        IEnumerable<string>? footerParameters = null,
        NotificationMetadata? metadata = null)
        : base(recipientId, title, body, NotificationChannel.WhatsApp, priority, metadata)
    {
        PhoneNumber = phoneNumber ?? throw new DomainException("WhatsApp phone number is required.");
        TemplateId = templateId;
        TemplateName = templateName;
        HeaderParameters = headerParameters?.ToList() ?? [];
        BodyParameters = bodyParameters?.ToList() ?? [];
        FooterParameters = footerParameters?.ToList() ?? [];
    }
}
