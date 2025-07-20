using Microsoft.AspNetCore.Http;

namespace Gargar.Common.Application.Interfaces;

public interface IS3Service
{
    Task<(string Url, string FileName)> UploadAsync(byte[] imageBytes, string fileName, string contentType);

    Task<(string Url, string FileName)> UploadAsync(Stream stream, string fileName, string contentType);

    Task<(string Url, string FileName)> UploadImageAsync(IFormFile file, string fileName, string contentType);

    Task<(string Url, string FileName)> UploadImageAsync(IFormFile file);

    Task<byte[]> DownloadAsync(string fileName);

    Task DeleteAsync(string fileName);

    Task<bool> ExistsAsync(string fileName);

    Task<string> GetUrlAsync(string fileName);
}