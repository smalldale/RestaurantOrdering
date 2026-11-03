namespace RestaurantOrdering.API.DTOs.MenuItem;

/// <summary>
/// Data required to create a menu item.
/// </summary>
public class CreateMenuItemDto
{
    /// <summary>
    /// Menu item name. Required, maximum 100 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional menu item description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unit price. Must be greater than zero.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Identifier of the category this item belongs to.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Whether the item can currently be ordered. Defaults to <c>true</c>.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
