using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

public class CheckoutOrderDto
{
    public PaymentMethod PaymentMethod { get; set; }
}
