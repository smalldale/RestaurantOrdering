using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.DTOs.MenuItem;

public class MenuItemQueryDto : PaginationQueryDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
}
