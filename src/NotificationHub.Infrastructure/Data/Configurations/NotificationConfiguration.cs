using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Core.Entities;
using NotificationHub.Core.Enums;
using System.Text.Json;

namespace NotificationHub.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Notification entity.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Primary key
        builder.HasKey(n => n.Id);

        // Required fields with max lengths
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        // Enums stored as integers
        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        // Optional recipient fields
        builder.Property(n => n.RecipientId)
            .HasMaxLength(256);

        builder.Property(n => n.RecipientName)
            .HasMaxLength(200);

        // Timestamps
        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt);
        builder.Property(n => n.SentAt);
        builder.Property(n => n.ReadAt);

        // Error tracking
        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(n => n.RetryCount)
            .IsRequired();

        // Metadata stored as JSON
        builder.Property(n => n.Metadata)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
            );

        // Indexes for common queries
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.Channel);
    }
}
