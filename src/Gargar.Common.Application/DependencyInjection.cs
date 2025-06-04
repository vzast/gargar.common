using Gargar.Common.Application.Interfaces;
using Gargar.Common.Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Gargar.Common.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
    // Register application services here
        services.AddScoped<IImageService, ImageService>();
        // services.AddScoped(typeof(IUoWService<,,,>), typeof(BaseUoWService<,,,>));
        return services;
    }
}