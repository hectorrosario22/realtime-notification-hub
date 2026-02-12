using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Notification aggregate.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Ignore(n => n.DomainEvents);

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.RecipientId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.Subject)
            .HasMaxLength(500);

        builder.Property(n => n.HtmlBody)
            .HasMaxLength(10000);

        builder.Property(n => n.SmsContent)
            .HasColumnName("SmsContent")
            .HasMaxLength(1600);

        builder.Property(n => n.TemplateName)
            .HasMaxLength(100);

        builder.Property(n => n.TemplateParameters)
            .HasColumnName("Parameters")
            .HasColumnType("text")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null));

        builder.Property(n => n.PushTitle)
            .HasColumnName("PushTitle")
            .HasMaxLength(200);

        builder.Property(n => n.PushContent)
            .HasColumnName("PushContent")
            .HasMaxLength(2000);

        builder.Property(n => n.CreatedAtUtc)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(n => n.UpdatedAtUtc)
            .HasColumnName("UpdatedAt");

        builder.Property(n => n.ScheduledAtUtc)
            .HasColumnName("ScheduledAt");

        builder.Property(n => n.SentAtUtc)
            .HasColumnName("SentAt");

        builder.Property(n => n.ReadAtUtc)
            .HasColumnName("ReadAt");

        builder.Property(n => n.RetryCount)
            .IsRequired();

        builder.Property(n => n.MaxRetries)
            .IsRequired();

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.CreatedAtUtc);
        builder.HasIndex(n => n.Channel);
    }
}
