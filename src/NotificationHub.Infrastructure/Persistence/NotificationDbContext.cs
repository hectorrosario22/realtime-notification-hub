using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var isRelational = Database.IsRelational();

        modelBuilder.Entity<Notification>(entity =>
        {
            if (isRelational)
                entity.ToTable("Notifications");

            entity.HasKey(n => n.Id);

            // TPH discriminator
            entity.HasDiscriminator(n => n.Channel)
                .HasValue<PushNotification>(NotificationChannel.Push)
                .HasValue<EmailNotification>(NotificationChannel.Email)
                .HasValue<SmsNotification>(NotificationChannel.Sms)
                .HasValue<WhatsAppNotification>(NotificationChannel.WhatsApp);

            // Base properties
            entity.Property(n => n.Title).IsRequired().HasMaxLength(500);
            entity.Property(n => n.Body).IsRequired();
            entity.Property(n => n.Channel).IsRequired().HasConversion<string>();
            entity.Property(n => n.Status).IsRequired().HasConversion<string>();
            entity.Property(n => n.Priority).IsRequired().HasConversion<string>();
            entity.Property(n => n.ErrorMessage).HasMaxLength(2000);

            // Metadata stored as JSON column via value converter
            if (isRelational)
            {
                var metadataConverter = new ValueConverter<NotificationMetadata, string>(
                    v => System.Text.Json.JsonSerializer.Serialize(
                        v.Data.ToDictionary(k => k.Key, k => k.Value),
                        (System.Text.Json.JsonSerializerOptions?)null),
                    v => NotificationMetadata.Create(
                        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                            v, (System.Text.Json.JsonSerializerOptions?)null)));

                var metadataComparer = new ValueComparer<NotificationMetadata>(
                    (a, b) => a != null && b != null && a.Equals(b),
                    v => v.GetHashCode(),
                    v => NotificationMetadata.Create(v.Data.ToDictionary(k => k.Key, k => k.Value)));

                entity.Property(n => n.Metadata)
                    .HasColumnName("Metadata")
                    .HasConversion(metadataConverter, metadataComparer)
                    .IsRequired(false);
            }
            else
            {
                entity.Ignore(n => n.Metadata);
            }

            // Indexes (relational only)
            if (isRelational)
            {
                entity.HasIndex(n => n.RecipientId);
                entity.HasIndex(n => n.Status);
                entity.HasIndex(n => n.CreatedAt);
            }

            // Ignore domain events (transient)
            entity.Ignore(n => n.DomainEvents);
        });

        // Email-specific configuration
        modelBuilder.Entity<EmailNotification>(entity =>
        {
            if (isRelational)
            {
                entity.OwnsOne(e => e.To, vo => vo.Property(v => v.Value).HasColumnName("EmailTo").HasMaxLength(254));
                entity.OwnsOne(e => e.From, vo => vo.Property(v => v.Value).HasColumnName("EmailFrom").HasMaxLength(254));
                entity.OwnsOne(e => e.ReplyTo, vo => vo.Property(v => v.Value).HasColumnName("EmailReplyTo").HasMaxLength(254));
                entity.OwnsMany(e => e.Cc, vo => { vo.ToJson("EmailCc"); vo.Property(v => v.Value).HasMaxLength(254); });
                entity.OwnsMany(e => e.Bcc, vo => { vo.ToJson("EmailBcc"); vo.Property(v => v.Value).HasMaxLength(254); });
                entity.PrimitiveCollection(e => e.AttachmentUrls).HasColumnName("AttachmentUrls");
            }
            else
            {
                entity.Ignore(e => e.To);
                entity.Ignore(e => e.From);
                entity.Ignore(e => e.ReplyTo);
                entity.Ignore(e => e.Cc);
                entity.Ignore(e => e.Bcc);
                entity.Ignore(e => e.AttachmentUrls);
            }
            entity.Property(e => e.IsHtml);
        });

        // SMS-specific configuration
        modelBuilder.Entity<SmsNotification>(entity =>
        {
            if (isRelational)
            {
                entity.OwnsOne(s => s.PhoneNumber, vo => vo.Property(v => v.Value).HasColumnName("SmsPhoneNumber").HasMaxLength(15));
                entity.OwnsOne(s => s.FromNumber, vo => vo.Property(v => v.Value).HasColumnName("SmsFromNumber").HasMaxLength(15));
            }
            else
            {
                entity.Ignore(s => s.PhoneNumber);
                entity.Ignore(s => s.FromNumber);
            }
        });

        // WhatsApp-specific configuration
        modelBuilder.Entity<WhatsAppNotification>(entity =>
        {
            if (isRelational)
            {
                entity.OwnsOne(w => w.PhoneNumber, vo => vo.Property(v => v.Value).HasColumnName("WhatsAppPhoneNumber").HasMaxLength(15));
                entity.PrimitiveCollection(w => w.HeaderParameters).HasColumnName("WhatsAppHeaderParams");
                entity.PrimitiveCollection(w => w.BodyParameters).HasColumnName("WhatsAppBodyParams");
                entity.PrimitiveCollection(w => w.FooterParameters).HasColumnName("WhatsAppFooterParams");
            }
            else
            {
                entity.Ignore(w => w.PhoneNumber);
                entity.Ignore(w => w.HeaderParameters);
                entity.Ignore(w => w.BodyParameters);
                entity.Ignore(w => w.FooterParameters);
            }
            entity.Property(w => w.TemplateId).HasMaxLength(200);
            entity.Property(w => w.TemplateName).HasMaxLength(200);
        });
    }
}
