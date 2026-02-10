using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the NotificationLog entity.
/// </summary>
public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        // Primary key
        builder.HasKey(nl => nl.Id);

        // Foreign key relationship to Notification
        builder.HasOne(nl => nl.Notification)
            .WithMany()
            .HasForeignKey(nl => nl.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required fields
        builder.Property(nl => nl.NotificationId)
            .IsRequired();

        builder.Property(nl => nl.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(nl => nl.EventType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(nl => nl.Timestamp)
            .IsRequired();

        // Optional fields with max lengths
        builder.Property(nl => nl.Message)
            .HasMaxLength(500);

        builder.Property(nl => nl.RequestData)
            .HasColumnType("text");

        builder.Property(nl => nl.ResponseData)
            .HasColumnType("text");

        builder.Property(nl => nl.ErrorDetails)
            .HasMaxLength(2000);

        // Indexes for efficient querying
        builder.HasIndex(nl => nl.NotificationId);
        builder.HasIndex(nl => nl.Channel);
        builder.HasIndex(nl => nl.EventType);
        builder.HasIndex(nl => nl.Timestamp);
    }
}
