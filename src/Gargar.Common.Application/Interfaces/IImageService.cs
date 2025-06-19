using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Gargar.Common.Application.Interfaces;

/// <summary>
/// Interface for image management services
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads an image to storage and stores its metadata in the database
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="altText">Optional alternative text for the image</param>
    /// <param name="description">Optional description of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The metadata of the uploaded image</returns>
    Task<ImageDTO> UploadImageAsync(
        IFormFile file,
        string altText,
        string description,
        bool isPublic = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from storage and removes its metadata from the database
    /// </summary>
    /// <param name="id">The ID of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the image was deleted; otherwise, false</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image by its filename
    /// </summary>
    /// <param name="fileName">The filename of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the image was deleted; otherwise, false</returns>
    Task<bool> DeleteImageByFileNameAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an image by its ID
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="refreshUrl">Whether to refresh the URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The image metadata, or null if not found</returns>
    Task<ImageDTO?> GetImageAsync(Guid id, bool refreshUrl = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URL for an image
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the image, or null if not found</returns>
    Task<string?> GetImageUrlAsync(Guid id, CancellationToken cancellationToken = default);

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
    Task<PagedList<ImageDTO>> GetImagesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        long? minSize = null,
        long? maxSize = null,
        CancellationToken cancellationToken = default);
}
