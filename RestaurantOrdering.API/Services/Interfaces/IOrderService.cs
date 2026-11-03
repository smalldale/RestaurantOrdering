using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponseDto<OrderResponseDto>> CreateAsync(
        CreateOrderDto dto);

    Task<ApiResponseDto<List<OrderResponseDto>>> GetAllAsync();

    Task<ApiResponseDto<OrderResponseDto>> GetByIdAsync(int id);

    Task<ApiResponseDto<OrderResponseDto>> UpdateCustomerNameAsync(
        int orderId,
        UpdateOrderCustomerNameDto dto);

    Task<ApiResponseDto<OrderResponseDto>> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusDto dto);

    Task<ApiResponseDto<OrderResponseDto>> UpdateItemQuantityAsync(
        int orderId,
        int orderItemId,
        UpdateOrderItemQuantityDto dto);

    Task<ApiResponseDto<bool>> DeleteAsync(int id);

    Task<ApiResponseDto<OrderResponseDto>> AddItemAsync(
        int orderId,
        AddOrderItemDto dto);

    Task<ApiResponseDto<OrderResponseDto>> RemoveItemAsync(
        int orderId,
        int orderItemId);

    Task<ApiResponseDto<OrderResponseDto>> CheckoutAsync(
        int orderId,
        CheckoutOrderDto dto);
}
