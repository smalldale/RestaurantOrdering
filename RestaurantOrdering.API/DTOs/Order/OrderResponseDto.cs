using RestaurantOrdering.API.Models.Enums;

namespace RestaurantOrdering.API.DTOs.Order;

/// <summary>
/// Order data returned by the API.
/// </summary>
public class OrderResponseDto
{
    /// <summary>
    /// Order identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Generated unique order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional customer name.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// UTC timestamp when the order was created.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Current lifecycle status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Sum of all line subtotals.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment method recorded at checkout, or <c>null</c> before checkout.
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// UTC timestamp of checkout, or <c>null</c> before checkout.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Lines that make up the order.
    /// </summary>
    public List<OrderItemResponseDto> OrderItems { get; set; } = [];
}
