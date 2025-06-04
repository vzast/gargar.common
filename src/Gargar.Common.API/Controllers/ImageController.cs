using Gargar.Common.Application.Interfaces;
using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Gargar.Common.API.Controllers;

/// <summary>
/// API controller for image management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ImageController"/> class
/// </remarks>
/// <param name="imageService">The image service</param>
/// <param name="logger">The logger</param>
[ApiController]
[Route("api/[controller]")]
public class ImageController(IImageService imageService, ILogger<ImageController> logger) : ControllerBase
{
    private readonly IImageService _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
    private readonly ILogger<ImageController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Uploads a new image
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="altText">Alternative text for the image</param>
    /// <param name="description">Description of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The metadata of the uploaded image</returns>
    /// <response code="201">Returns the newly created image metadata</response>
    /// <response code="400">If the file is not a valid image</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImageDTO>> UploadImage(
        [Required] IFormFile file,
        [FromForm] string altText = "Default Alt",
        [FromForm] string description = "Default Description",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uploading image with filename {Filename}", file.FileName);
            
            var imageDto = await _imageService.UploadImageAsync(
                file, 
                altText, 
                description,
                cancellationToken);
            
            return CreatedAtAction(nameof(GetImage), new { id = imageDto.Id }, imageDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid image upload attempt");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while uploading the image" });
        }
    }

    /// <summary>
    /// Retrieves an image by ID
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="refreshUrl">Whether to refresh the URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The image metadata</returns>
    /// <response code="200">Returns the image metadata</response>
    /// <response code="404">If the image is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageDTO>> GetImage(
        [FromRoute] Guid id,
        [FromQuery] bool refreshUrl = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting image with ID {Id}", id);
        
        var image = await _imageService.GetImageAsync(id, refreshUrl, cancellationToken);
        
        if (image == null)
        {
            _logger.LogWarning("Image with ID {Id} not found", id);
            return NotFound();
        }
        
        return Ok(image);
    }

    /// <summary>
    /// Gets the URL for an image
    /// </summary>
    /// <param name="id">The ID of the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the image</returns>
    /// <response code="200">Returns the image URL</response>
    /// <response code="404">If the image is not found</response>
    [HttpGet("{id:guid}/url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetImageUrl(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting URL for image with ID {Id}", id);
        
        var url = await _imageService.GetImageUrlAsync(id, cancellationToken);
        
        if (url == null)
        {
            _logger.LogWarning("Image with ID {Id} not found", id);
            return NotFound();
        }
        
        return Ok(url);
    }

    /// <summary>
    /// Retrieves a paged list of images
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="minSize">Minimum size in bytes</param>
    /// <param name="maxSize">Maximum size in bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paged list of images</returns>
    /// <response code="200">Returns the paged list of images</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedList<ImageDTO>>> GetImages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? minSize = null,
        [FromQuery] long? maxSize = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting images page {PageNumber} with size {PageSize}", pageNumber, pageSize);
        
        if (pageNumber < 1)
            return BadRequest(new { error = "Page number must be greater than 0" });
            
        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        
        var images = await _imageService.GetImagesAsync(
            pageNumber,
            pageSize,
            minSize,
            maxSize,
            cancellationToken);

        // Set pagination headers
        try
        {
            Response.Headers.Add("X-Pagination-TotalCount", images.TotalCount.ToString());
            Response.Headers.Add("X-Pagination-PageSize", images.PageSize.ToString());
            Response.Headers.Add("X-Pagination-CurrentPage", images.PageNumber.ToString());
            Response.Headers.Add("X-Pagination-TotalPages", images.TotalPages.ToString());
        }
        catch (Exception)
        {

            throw;
        }
        
        return Ok(images);
    }

    /// <summary>
    /// Deletes an image by ID
    /// </summary>
    /// <param name="id">The ID of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the image was successfully deleted</response>
    /// <response code="404">If the image was not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting image with ID {Id}", id);
        
        var success = await _imageService.DeleteAsync(id, cancellationToken);
        
        if (!success)
        {
            _logger.LogWarning("Image with ID {Id} not found for deletion", id);
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Deletes an image by filename
    /// </summary>
    /// <param name="fileName">The filename of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the image was successfully deleted</response>
    /// <response code="404">If the image was not found</response>
    [HttpDelete("by-filename/{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImageByFileName(
        [FromRoute] string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting image with filename {Filename}", fileName);
        
        try
        {
            var success = await _imageService.DeleteImageByFileNameAsync(fileName, cancellationToken);
            
            if (!success)
            {
                _logger.LogWarning("Image with filename {Filename} not found for deletion", fileName);
                return NotFound();
            }
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid filename provided");
            return BadRequest(new { error = ex.Message });
        }
    }
}
