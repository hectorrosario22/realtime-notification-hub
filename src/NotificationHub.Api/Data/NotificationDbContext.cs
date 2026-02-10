using Microsoft.EntityFrameworkCore;
using NotificationHub.Api.Entities;

namespace NotificationHub.Api.Data;

/// <summary>
/// Database context for the Notification Hub application.
/// </summary>
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Base notifications table (includes all derived types via TPH)
    /// </summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Email notifications (filtered view of Notifications table)
    /// </summary>
    public DbSet<EmailNotification> EmailNotifications => Set<EmailNotification>();

    /// <summary>
    /// SMS notifications (filtered view of Notifications table)
    /// </summary>
    public DbSet<SmsNotification> SmsNotifications => Set<SmsNotification>();

    /// <summary>
    /// WhatsApp notifications (filtered view of Notifications table)
    /// </summary>
    public DbSet<WhatsAppNotification> WhatsAppNotifications => Set<WhatsAppNotification>();

    /// <summary>
    /// Push notifications (filtered view of Notifications table)
    /// </summary>
    public DbSet<PushNotification> PushNotifications => Set<PushNotification>();

    /// <summary>
    /// Notification audit logs
    /// </summary>
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
