using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Gargar.Common.Application.Mapper;

[Mapper]
public partial class ImageMappers
{
    public partial ImageDTO Map(Image source);

    public partial IEnumerable<ImageDTO> Map(IEnumerable<Image> source);

    public partial PagedList<ImageDTO> Map(PagedList<Image> source);
}