using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to check out a pending order.
/// </summary>
public class CheckoutOrderDto
{
    /// <summary>
    /// Payment method recorded for the order.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
}
