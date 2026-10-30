
namespace RestaurantOrdering.Services.Enums;

public enum OrderOperationResult
{
    Success,
    OrderNotFound,
    MenuItemNotFound,
    MenuItemUnavailable,
    InvalidQuantity,
    OrderNotPending,
    OrderItemNotFound,
    OrderHasNoItems
}
