using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.DTOs.MenuItem;

/// <summary>
/// Query parameters for listing menu items.
/// </summary>
public class MenuItemQueryDto : PaginationQueryDto
{
    /// <summary>
    /// Optional case-insensitive text used to filter menu item names.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional category identifier used to filter the results.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Optional availability filter.
    /// </summary>
    public bool? IsAvailable { get; set; }
}
