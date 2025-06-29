using Gargar.Common.Application.Interfaces;
using Gargar.Common.Infrastructure.Email;
using Gargar.Common.Infrastructure.S3;
using Gargar.Common.Infrastructure.S3.Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Gargar.Common.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Email Service Configuration

        services.Configure<EmailOptions>(configuration.GetSection("EmailOptions").Bind);
        services.AddSingleton<IEmailService, EmailService>();

        #endregion Email Service Configuration

        services.Configure<S3Options>(configuration.GetSection("S3Options").Bind);
        services.AddSingleton<IMinioClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<S3Options>>().Value;
            return new MinioClient()
                .WithEndpoint(options.ServiceURL, 9000)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(false) // Adjust based on your setup
                .Build();
        });
        services.AddScoped<IS3ImgService, MinioService>();

        return services;
    }
}