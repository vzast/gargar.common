using Gargar.Common.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Gargar.Common.Infrastructure.S3.Minio;

public static class MinioExtentions
{
    public static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        var s3Options = configuration.GetSection("S3Options").Get<S3Options>();

        services.Configure<S3Options>(configuration.GetSection("S3Options").Bind);
        if (s3Options is null)
        {
            throw new ArgumentNullException("s3Options is null");
        }
        services.AddSingleton(_ =>
                 new MinioClient()
                     .WithEndpoint(s3Options.ServiceURL, s3Options.Port)
                     .WithCredentials(s3Options.AccessKey, s3Options.SecretKey)
                     .WithSSL(false)
                     .Build());

        services.AddSingleton<IS3Service, MinioService>();

        return services;
    }
}