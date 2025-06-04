using Gargar.Common.Domain.Helpers.Enums;
using System.ComponentModel.DataAnnotations;

namespace Gargar.Common.Domain.Helpers;

public class File
{
    public FileTypes Type { get; internal set; }
    public long Size { get; set; }
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    [MaxLength(512)]
    public string? Location { get; set; }
}
