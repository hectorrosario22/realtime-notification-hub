using Microsoft.EntityFrameworkCore;
using NotificationHub.Core.Entities;

namespace NotificationHub.Infrastructure.Data;

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
    /// Notifications table
    /// </summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
