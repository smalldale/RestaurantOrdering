using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to change an order's status.
/// </summary>
public class UpdateOrderStatusDto
{
    /// <summary>
    /// Target status. Must be a defined <see cref="OrderStatus"/> value
    /// and a valid transition from the current status.
    /// </summary>
    public OrderStatus Status { get; set; }
}
