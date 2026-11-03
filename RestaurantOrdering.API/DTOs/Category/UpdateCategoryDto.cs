namespace RestaurantOrdering.API.DTOs.Category;

/// <summary>
/// Data required to update a category.
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Category name. Required, maximum 100 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional category description, maximum 500 characters.
    /// </summary>
    public string? Description { get; set; }
}
