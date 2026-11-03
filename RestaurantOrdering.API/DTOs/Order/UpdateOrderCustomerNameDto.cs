namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Data required to update an order's customer name.
/// </summary>
public class UpdateOrderCustomerNameDto
{
    /// <summary>
    /// New customer name, maximum 100 characters. Empty or whitespace clears it.
    /// </summary>
    public string? CustomerName { get; set; }
}
