namespace RestaurantOrdering.API.DTOs.Common;

/// <summary>
/// A single page of results together with paging metadata.
/// </summary>
/// <typeparam name="T">Type of the items in the page.</typeparam>
public class PagedResponseDto<T>
{
    /// <summary>
    /// Items contained in the current page.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// One-based number of the current page.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Maximum number of items requested per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
