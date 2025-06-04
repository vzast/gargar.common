using Gargar.Common.Application.Interfaces;
using Gargar.Common.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gargar.Common.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Email Service Configuration

        services.Configure<EmailOptions>(configuration.GetSection("EmailOptions").Bind);
        services.AddSingleton<IEmailService, EmailService>();

        #endregion Email Service Configuration

        return services;
    }
}