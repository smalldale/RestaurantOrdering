namespace RestaurantOrdering.API.DTOs.Category;

/// <summary>
/// Category data returned by the API.
/// </summary>
public class CategoryResponseDto
{
    /// <summary>
    /// Category identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional category description.
    /// </summary>
    public string? Description { get; set; }
}
