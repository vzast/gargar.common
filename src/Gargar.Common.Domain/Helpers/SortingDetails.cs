namespace Gargar.Common.Domain.Helpers;

public class SortingDetails
{
    //
    // Summary:
    //     Sorting statements
    public List<SortItem> SortItems { get; set; }

    public SortingDetails()
    {
    }

    public SortingDetails(List<SortItem> sortItems)
    {
        if (sortItems.Count == 0)
        {
            throw new Exception("sort by items are null");
        }
        SortItems = sortItems;
    }

    public SortingDetails(SortItem sortItem)
    {
        Guard.NotNull(sortItem, "sortItem");
        Guard.NotEmpty(sortItem.SortBy, "SortBy");
        SortItems = [sortItem];
    }
}

public class SortItem
{
    //
    // Summary:
    //     Sorting field name
    public string SortBy { get; set; }

    //
    // Summary:
    //     Sorting direction (ascending, descending)
    public SortDirection SortDirection { get; set; }

    public SortItem()
    {
    }

    public SortItem(string sortBy, SortDirection sortDirection)
    {
        Guard.NotEmpty(sortBy, nameof(sortBy));
        SortBy = sortBy;
        SortDirection = sortDirection;
    }
}

public enum SortDirection
{
    //
    // Summary:
    //     Ascending sorting direction
    Ascending = 0,

    //
    // Summary:
    //     Descending sorting direction
    Descending = 1
}