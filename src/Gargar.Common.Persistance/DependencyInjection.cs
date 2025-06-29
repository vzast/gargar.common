using Gargar.Common.Application.Interfaces;
using Gargar.Common.Domain.Identity;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Database;
using Gargar.Common.Persistance.Repository;
using Gargar.Common.Persistance.UoW;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gargar.Common.Persistance;

public static class DependencyInjection
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }

    public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

        services.AddIdentityApiEndpoints<User>(options =>
        {
            options = configuration.GetSection("IdentityOptions").Get<IdentityOptions>()!;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
           .AddRoles<Role>()
       .AddEntityFrameworkStores<AppDbContext>()
       .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/accessdenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(1);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static WebApplication UseIdentityServices(this WebApplication app)
    {
        app.ApplyMigrations();
        app.MapIdentityApi<User>();
        return app;
    }
}