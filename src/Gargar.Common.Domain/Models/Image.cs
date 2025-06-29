using Gargar.Common.Domain.Helpers.Enums;
using System.ComponentModel.DataAnnotations;

namespace Gargar.Common.Domain.Models;

public class Image : Gargar.Common.Domain.Helpers.File
{
    [Key]
    public Guid Id { get; set; }

    public Image()
    {
        Type = FileTypes.Image;
    }

    [MaxLength(512)]
    public string Url { get; set; } = null!;// URL to the image file

    [MaxLength(128)]
    public string AltText { get; set; } = null!;

    [MaxLength(128)]
    public string Description { get; set; } = null!;

    public DateTime UploadedAt { get; set; }
}