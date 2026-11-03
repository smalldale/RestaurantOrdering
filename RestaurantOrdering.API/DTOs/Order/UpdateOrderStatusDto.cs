using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
