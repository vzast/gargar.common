namespace Gargar.Common.Domain.Helpers;

public class PagingDetails
{
    public int PageIndex { get; set; } = 1;
    public int PageNumber => PageIndex + 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }

    public int TotalPages
    {
        get
        {
            int pages = TotalCount / PageSize;
            if (TotalCount % PageSize > 0)
            {
                pages++;
            }
            return pages;
        }
    }

    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < TotalPages - 1;
    public bool IsFirstPage => PageIndex == 0;
    public bool IsLastPage => PageIndex >= TotalPages - 1;
    public bool IsEmpty => TotalCount == 0;
    public bool SortDescending { get; set; } = false;

    public PagingDetails()
    {
    }

    public PagingDetails(int pageIndex, int pageSize, int totalCount)
    {
        PageIndex = pageIndex >= 0 ? pageIndex : 0;
        PageSize = pageSize >= 0 ? pageSize : 10;
        TotalCount = totalCount >= 0 ? totalCount : 0;
    }
}