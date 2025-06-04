using Gargar.Common.Application.Interfaces;
using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Helpers;
using Gargar.Common.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Gargar.Common.Application.Mapper;

[Mapper]
public partial class ImageMappers : IMapper<Image, ImageDTO>
{
    public partial ImageDTO Map(Image source);

    public partial IEnumerable<ImageDTO> MapCollection(IEnumerable<Image> source);

    public partial PagedList<ImageDTO> MapPaged(PagedList<Image> source);
}
