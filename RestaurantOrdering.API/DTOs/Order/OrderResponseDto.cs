using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

public class OrderResponseDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }

    public List<OrderItemResponseDto> OrderItems { get; set; } = [];
}
