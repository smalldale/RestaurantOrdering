namespace RestaurantOrdering.API.DTOs.Common;

/// <summary>
/// Common paging parameters for list endpoints.
/// </summary>
public class PaginationQueryDto
{
    /// <summary>
    /// One-based page number. Defaults to 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 10; maximum 100.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
