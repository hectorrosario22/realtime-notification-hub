using Microsoft.Extensions.DependencyInjection;

namespace NotificationHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Assemblies = [typeof(DependencyInjection)];
        });

        return services;
    }
}
