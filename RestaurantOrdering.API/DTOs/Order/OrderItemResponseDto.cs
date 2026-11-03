namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Order line data returned by the API.
/// </summary>
public class OrderItemResponseDto
{
    /// <summary>
    /// Order line identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the referenced menu item.
    /// </summary>
    public int MenuItemId { get; set; }

    /// <summary>
    /// Name of the referenced menu item.
    /// </summary>
    public string MenuItemName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price captured when the line was created.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line total, calculated as <c>Quantity * UnitPrice</c>.
    /// </summary>
    public decimal Subtotal { get; set; }
}
