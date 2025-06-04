namespace Gargar.Common.Application.Interfaces
{
    public interface IS3ImgService
    {
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string contentType);

        Task<byte[]> DownloadImageAsync(string fileName);

        Task DeleteImageAsync(string fileName);

        Task<bool> ImageExistsAsync(string fileName);

        Task<string> GetImageUrlAsync(string fileName);
    }
}