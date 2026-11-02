namespace RestaurantOrdering.API.DTOs.Order;

public class AddOrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}
