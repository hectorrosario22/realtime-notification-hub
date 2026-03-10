using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationHub.Infrastructure.Persistence;

namespace NotificationHub.Api.Tests.Infrastructure;

public class NotificationHubWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core and Npgsql-related registrations
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<NotificationDbContext>)
                || d.ServiceType == typeof(DbContextOptions)
                || d.ServiceType == typeof(NotificationDbContext)
                || (d.ServiceType.IsGenericType
                    && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                || d.ServiceType.FullName?.Contains("Npgsql") == true
                || d.ImplementationType?.FullName?.Contains("Npgsql") == true
                || d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true
            ).ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            // Re-register with InMemory provider (no JSON owned types support,
            // but EnsureCreated will skip JSON constraints)
            var dbName = $"NotificationHub_Test_{Guid.NewGuid()}";
            services.AddDbContext<NotificationDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }
}
