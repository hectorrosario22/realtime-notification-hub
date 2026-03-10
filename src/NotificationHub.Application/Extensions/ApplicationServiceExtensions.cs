using Microsoft.Extensions.DependencyInjection;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Services;

namespace NotificationHub.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
