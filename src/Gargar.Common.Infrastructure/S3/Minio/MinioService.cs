using System;
using System.IO;
using System.Threading.Tasks;
using Gargar.Common.Application.Interfaces;
using Gargar.Common.Infrastructure.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Gargar.Common.Infrastructure.S3.Minio;

/// <summary>
/// Implementation of image service using MinIO S3-compatible storage
/// </summary>
public class MinioService : IImgService
{
    private readonly S3Options _options;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioService> _logger;
    private readonly TimeSpan _defaultUrlExpiry = TimeSpan.FromDays(7);

    /// <summary>
    /// Initializes a new instance of the <see cref="MinioService"/> class
    /// </summary>
    /// <param name="options">S3 configuration options</param>
    /// <param name="minioClient">MinIO client instance</param>
    /// <param name="logger">Logger instance</param>
    public MinioService(
        IOptions<S3Options> options,
        IMinioClient minioClient,
        ILogger<MinioService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string contentType)
    {
        try
        {
            await EnsureBucketExistsAsync();

            string objectName = GenerateUniqueFileName(fileName);
            using var stream = new MemoryStream(imageBytes);

            _logger.LogInformation("Uploading image {FileName} to bucket {BucketName}", objectName, _options.BucketName);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType));

            _logger.LogInformation("Successfully uploaded image {FileName}", objectName);

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> DownloadImageAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Downloading image {FileName} from bucket {BucketName}", fileName, _options.BucketName);

            using var memoryStream = new MemoryStream();

            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

            _logger.LogInformation("Successfully downloaded image {FileName}", fileName);

            return memoryStream.ToArray();
        }
        catch (ObjectNotFoundException)
        {
            _logger.LogWarning("Image {FileName} not found in bucket {BucketName}", fileName, _options.BucketName);
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteImageAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Deleting image {FileName} from bucket {BucketName}", fileName, _options.BucketName);

            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName));

            _logger.LogInformation("Successfully deleted image {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ImageExistsAsync(string fileName)
    {
        try
        {
            _logger.LogDebug("Checking if image {FileName} exists in bucket {BucketName}", fileName, _options.BucketName);

            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName));

            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if image {FileName} exists", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<string> GetImageUrlAsync(string fileName)
    {
        // Use default expiry time (7 days)
        return GetPresignedUrlAsync(fileName, _defaultUrlExpiry);
    }

    /// <summary>
    /// Generates a presigned URL for the specified file with custom expiration time
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    /// <param name="expiryDuration">The duration for which the URL should be valid</param>
    /// <returns>A presigned URL that can be used to access the file</returns>
    public async Task<string> GetPresignedUrlAsync(string fileName, TimeSpan expiryDuration)
    {
        try
        {
            _logger.LogInformation("Generating presigned URL for image {FileName} with expiry {Expiry}",
                fileName, expiryDuration);

            // Ensure expiry is positive and not excessive
            int expirySeconds = (int)Math.Min(Math.Max(expiryDuration.TotalSeconds, 1), 604800); // Max 7 days (AWS limitation)

            string url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName)
                .WithExpiry(expirySeconds));

            _logger.LogInformation("Successfully generated presigned URL for image {FileName}", fileName);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for image {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Ensures that the configured bucket exists, creating it if necessary
    /// </summary>
    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            bool bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs()
                .WithBucket(_options.BucketName));

            if (!bucketExists)
            {
                _logger.LogInformation("Creating bucket {BucketName}", _options.BucketName);

                var makeBucketArgs = new MakeBucketArgs().WithBucket(_options.BucketName);

                // If region is specified, use it
                if (!string.IsNullOrEmpty(_options.Region))
                {
                    makeBucketArgs.WithLocation(_options.Region);
                }

                await _minioClient.MakeBucketAsync(makeBucketArgs);

                _logger.LogInformation("Successfully created bucket {BucketName}", _options.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket {BucketName} exists", _options.BucketName);
            throw;
        }
    }

    /// <summary>
    /// Generates a unique file name to prevent collisions
    /// </summary>
    /// <param name="originalFileName">The original file name</param>
    /// <returns>A unique file name</returns>
    private static string GenerateUniqueFileName(string originalFileName)
    {
        string extension = Path.GetExtension(originalFileName);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        return $"{fileNameWithoutExtension}_{Guid.NewGuid()}{extension}";
    }
}