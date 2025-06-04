using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gargar.Common.Application.POCO;

public class ImageDTO
{
/// <summary>
    /// Gets or sets the unique identifier for the image.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the URL of the image.
    /// </summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the name of the image.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the size of the image in bytes.
    /// </summary>
    public long Size { get; set; }
}
