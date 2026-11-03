namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to create an order.
/// </summary>
public class CreateOrderDto
{
    /// <summary>
    /// Optional customer name, maximum 100 characters.
    /// </summary>
    public string? CustomerName { get; set; }
}
