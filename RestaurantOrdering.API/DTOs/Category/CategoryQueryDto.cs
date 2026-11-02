using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.DTOs.Category;

public class CategoryQueryDto : PaginationQueryDto
{
    public string? SearchTerm { get; set; }
}
