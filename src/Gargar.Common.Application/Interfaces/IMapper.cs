using Gargar.Common.Domain.Helpers;

namespace Gargar.Common.Application.Interfaces;

public interface IMapper<TSource, TDestination> where TDestination : class
{
    /// <summary>
    /// Maps an object of type TSource to an object of type TDestination
    /// </summary>
    TDestination Map(TSource source);

    /// <summary>
    /// Maps a collection of objects of type TSource to a collection of objects of type TDestination
    /// </summary>
    IEnumerable<TDestination> MapCollection(IEnumerable<TSource> source);

    /// <summary>
    /// Maps a pagedlist of objects of type TSource to a collection of objects of type TDestination
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    PagedList<TDestination> MapPaged(PagedList<TSource> source);
}