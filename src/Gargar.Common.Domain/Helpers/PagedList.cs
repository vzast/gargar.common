namespace Gargar.Common.Domain.Helpers;

public class PagedList<T>
{
    public PagingDetails PagingDetails { get; set; } = new PagingDetails();
    public List<T> List { get; set; } = [];

    public PagedList()
    {
    }

    public PagedList(IEnumerable<T> list, PagingDetails pagingDetails)
    {
        List = [.. list];
        PagingDetails = pagingDetails;
    }

    public PagedList(IEnumerable<T> list, int pageIndex, int pageSize, int totalCount)
    {
        List = [.. list];
        PagingDetails = new PagingDetails(pageIndex, pageSize, totalCount);
    }

    public PagedList(IQueryable<T> list, int pageIndex, int pageSize)
    {
        PagingDetails = new PagingDetails(pageIndex, pageSize, list.Count());
        List = [.. list.Skip(PagingDetails.PageIndex * PagingDetails.PageSize).Take(PagingDetails.PageSize)];
    }
}