using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.DTOs.Category;

/// <summary>
/// Query parameters for listing categories.
/// </summary>
public class CategoryQueryDto : PaginationQueryDto
{
    /// <summary>
    /// Optional case-insensitive text used to filter category names.
    /// </summary>
    public string? SearchTerm { get; set; }
}
