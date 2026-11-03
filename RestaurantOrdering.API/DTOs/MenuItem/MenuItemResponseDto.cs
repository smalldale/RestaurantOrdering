namespace RestaurantOrdering.API.DTOs.MenuItem;

/// <summary>
/// Menu item data returned by the API.
/// </summary>
public class MenuItemResponseDto
{
    /// <summary>
    /// Menu item identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Menu item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional menu item description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unit price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether the item can currently be ordered.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Identifier of the category this item belongs to.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Name of the category this item belongs to.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
}
