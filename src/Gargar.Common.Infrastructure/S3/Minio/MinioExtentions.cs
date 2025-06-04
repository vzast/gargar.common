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

        services.AddSingleton(_ =>
                 new MinioClient()
                     .WithEndpoint(s3Options.ServiceURL, 9000)
                     .WithCredentials(s3Options.AccessKey, s3Options.SecretKey)
                     .WithSSL(false)
                     .Build());
        //add img service example
        //services.AddSingleton<IImgService, MinioService>();

        return services;
    }
}