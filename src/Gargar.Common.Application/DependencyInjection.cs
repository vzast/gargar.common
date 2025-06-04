using Microsoft.Extensions.DependencyInjection;

namespace Gargar.Common.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services here
        // Example: services.AddScoped<IMyService, MyService>();

        return services;
    }
}