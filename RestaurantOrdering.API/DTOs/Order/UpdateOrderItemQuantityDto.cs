namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to change the quantity of an existing order line.
/// </summary>
public class UpdateOrderItemQuantityDto
{
    /// <summary>
    /// New quantity. Must be greater than zero.
    /// </summary>
    public int Quantity { get; set; }
}
