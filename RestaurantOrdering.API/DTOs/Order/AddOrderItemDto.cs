namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to add a menu item to an order.
/// </summary>
public class AddOrderItemDto
{
    /// <summary>
    /// Identifier of the menu item to add.
    /// </summary>
    public int MenuItemId { get; set; }

    /// <summary>
    /// Quantity to add. Must be greater than zero.
    /// </summary>
    public int Quantity { get; set; }
}
