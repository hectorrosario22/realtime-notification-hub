using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;
using System.Text.Json;

namespace NotificationHub.Api.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Notification entity hierarchy using TPH.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Configure Table-Per-Hierarchy (TPH) mapping strategy
        builder.UseTphMappingStrategy();

        // Configure discriminator column
        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<EmailNotification>("Email")
            .HasValue<SmsNotification>("Sms")
            .HasValue<WhatsAppNotification>("WhatsApp")
            .HasValue<PushNotification>("Push");

        // Primary key
        builder.HasKey(n => n.Id);

        // Enums stored as integers
        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        // Optional recipient field
        builder.Property(n => n.RecipientId)
            .HasMaxLength(256);

        // Timestamps
        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt);
        builder.Property(n => n.SentAt);

        // Error tracking
        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(n => n.RetryCount)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.Channel);
    }
}

/// <summary>
/// Entity Framework configuration for the EmailNotification entity.
/// </summary>
public class EmailNotificationConfiguration : IEntityTypeConfiguration<EmailNotification>
{
    public void Configure(EntityTypeBuilder<EmailNotification> builder)
    {
        builder.Property(e => e.Subject)
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .HasMaxLength(10000);
    }
}

/// <summary>
/// Entity Framework configuration for the SmsNotification entity.
/// </summary>
public class SmsNotificationConfiguration : IEntityTypeConfiguration<SmsNotification>
{
    public void Configure(EntityTypeBuilder<SmsNotification> builder)
    {
        builder.Property(s => s.Content)
            .HasColumnName("SmsContent")
            .HasMaxLength(1600);
    }
}

/// <summary>
/// Entity Framework configuration for the WhatsAppNotification entity.
/// </summary>
public class WhatsAppNotificationConfiguration : IEntityTypeConfiguration<WhatsAppNotification>
{
    public void Configure(EntityTypeBuilder<WhatsAppNotification> builder)
    {
        builder.Property(w => w.TemplateName)
            .HasMaxLength(100);

        builder.Property(w => w.Parameters)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
            );
    }
}

/// <summary>
/// Entity Framework configuration for the PushNotification entity.
/// </summary>
public class PushNotificationConfiguration : IEntityTypeConfiguration<PushNotification>
{
    public void Configure(EntityTypeBuilder<PushNotification> builder)
    {
        builder.Property(p => p.Title)
            .HasColumnName("PushTitle")
            .HasMaxLength(200);

        builder.Property(p => p.Content)
            .HasColumnName("PushContent")
            .HasMaxLength(2000);

        builder.Property(p => p.ReadAt);
    }
}
