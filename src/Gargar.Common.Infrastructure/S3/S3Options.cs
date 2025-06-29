namespace Gargar.Common.Infrastructure.S3;

public class S3Options
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ServiceURL { get; set; } = string.Empty;
    public bool WithSSL { get; set; }

    public string? PublicUrl =>
        !string.IsNullOrEmpty(ServiceURL) ? $"{(WithSSL ? "https" : "http")}://{ServiceURL}/{BucketName}/" : null;
}