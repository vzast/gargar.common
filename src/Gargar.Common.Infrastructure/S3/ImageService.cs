using Gargar.Common.Application.Interfaces;
using Gargar.Common.Application.Mapper;
using Gargar.Common.Application.POCO;
using Gargar.Common.Application.Service;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Gargar.Common.Infrastructure.S3;

public class ImageService(
    IUnitOfWork unitOfWork,
    IS3ImgService storageService) : BaseUoWService<Image, Guid, ImageDTO, ImageMappers>(unitOfWork), IImageService
{
    private readonly IS3ImgService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    /// <summary>
    /// Uploads an image to storage and stores its metadata in the database
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="altText">Optional alternative text for the image</param>
    /// <param name="description">Optional description of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The metadata of the uploaded image</returns>
    public async Task<ImageDTO> UploadImageAsync(
        IFormFile file,
        string altText = "Deffauld Alt",
        string description = "Deffauld Description",
        bool isPublic = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required and must not be empty", nameof(file));

        // Validate file type
        string contentType = file.ContentType.ToLowerInvariant();
        if (!contentType.StartsWith("image/"))
            throw new ArgumentException("Only image files are allowed", nameof(file));

        // Convert to byte array
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        byte[] imageData = memoryStream.ToArray();

        // Upload to storage
        (string PublicUrl, string FileName) = await _storageService.UploadImageAsync(
            imageData,
            file.FileName,
            contentType);
        string imageUrl = PublicUrl;
        // Get URL
        if (!isPublic)
        {
            imageUrl = await _storageService.GetImageUrlAsync(FileName);

        }



        // Create database record
        var image = new Image
        {
            Id = Guid.NewGuid(),
            Name = FileName,
            Size = file.Length,
            AltText = altText,
            Description = description,
            Url = PublicUrl,
            UploadedAt = DateTime.UtcNow
        };

        // Use the base class method to add the entity
        return await base.AddAsync(image, cancellationToken);
    }

    /// <summary>
    /// Deletes an image from storage and removes its metadata from the database
    /// </summary>
    /// <param name="id">The ID of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the image was deleted; otherwise, false</returns>
    public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await Repository.GetByIdAsync(id);
        if (image == null)
            return false;

        // Delete from storage
        await _storageService.DeleteImageAsync(image.Name);

        // Use base method to delete from database
        return await base.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    /// Deletes an image by its filename
    /// </summary>
    /// <param name="fileName">The filename of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the image was deleted; otherwise, false</returns>
    public async Task<bool> DeleteImageByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Filename cannot be null or empty", nameof(fileName));

        // Find the image in the database
        var images = await Repository.GetAllAsync(i => i.Name == fileName);
        var image = images.FirstOrDefault();

        if (image == null)
        {
            // If not in database but in storage, delete from storage
            if (await _storageService.ImageExistsAsync(fileName))
            {
                await _storageService.DeleteImageAsync(fileName);
                return true;
            }
            return false;
        }

        // Delete from storage
        await _storageService.DeleteImageAsync(fileName);

        // Use the base method to delete from database
        return await base.DeleteAsync(image.Id, cancellationToken);
    }

    /// <summary>
    /// Gets an image by its ID
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="refreshUrl">Whether to refresh the URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The image metadata, or null if not found</returns>
    public async Task<ImageDTO?> GetImageAsync(Guid id, bool refreshUrl = false, CancellationToken cancellationToken = default)
    {
        var image = await Repository.GetByIdAsync(id);
        if (image == null)
            return null;

        if (refreshUrl)
        {
            // Generate a new URL
            image.Url = await _storageService.GetImageUrlAsync(image.Name);

            // Update in database
            Repository.Update(image);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return _mapper.Map(image);
    }

    /// <summary>
    /// Gets the URL for an image
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the image, or null if not found</returns>
    public async Task<string?> GetImageUrlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await Repository.GetByIdAsync(id);
        if (image == null)
            return null;

        // Generate a fresh URL
        return await _storageService.GetImageUrlAsync(image.Name);
    }

    /// <summary>
    /// Gets all images with optional filtering by size and type
    /// </summary>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="contentType">Optional content type filter</param>
    /// <param name="minSize">Optional minimum size filter</param>
    /// <param name="maxSize">Optional maximum size filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of image metadata</returns>
    public async Task<IEnumerable<ImageDTO>> GetImagesAsync(
        int maxResults = 100,
        long? minSize = null,
        long? maxSize = null,
        CancellationToken cancellationToken = default)
    {
        // Build a filter expression
        Expression<Func<Image, bool>> predicate = i =>
            (!minSize.HasValue || i.Size >= minSize.Value) &&
            (!maxSize.HasValue || i.Size <= maxSize.Value);

        // Use the base GetAllAsync method to get filtered images
        var images = await base.GetAllAsync(predicate, cancellationToken);

        // Apply the limit and return
        return images.Take(maxResults);
    }

    /// <summary>
    /// Gets a paged list of images with optional filtering
    /// </summary>
    /// <param name="contentType">Optional content type filter</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="minSize">Optional minimum size filter</param>
    /// <param name="maxSize">Optional maximum size filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paged list of image metadata</returns>
    public async Task<PagedList<ImageDTO>> GetImagesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        long? minSize = null,
        long? maxSize = null,
        CancellationToken cancellationToken = default)
    {
        // Build the filter expression
        Expression<Func<Image, bool>> predicate = i =>
            (!minSize.HasValue || i.Size >= minSize.Value) &&
            (!maxSize.HasValue || i.Size <= maxSize.Value);

        // Define the sort order (newest first by default)

        // Use the base class method to get paged results
        return await base.GetPagedAsync(predicate, OrderBy, pageNumber, pageSize, cancellationToken);
    }
    static IOrderedQueryable<Image> OrderBy(IQueryable<Image> q) => q.OrderByDescending(i => i.UploadedAt);

    /// <summary>
    /// Generates a unique filename based on the original filename
    /// </summary>
    /// <param name="originalFileName">The original filename</param>
    /// <returns>A unique filename</returns>

}