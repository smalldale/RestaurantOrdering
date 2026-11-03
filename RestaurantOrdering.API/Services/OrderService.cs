using AutoMapper;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.Order;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Models.Enums;
using RestaurantOrdering.API.Repositories.Interfaces;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<OrderResponseDto>> CreateAsync(
        CreateOrderDto dto)
    {
        var order = _mapper.Map<Order>(dto);

        order.OrderNumber = GenerateOrderNumber();
        order.OrderDate = DateTime.UtcNow;
        order.Status = OrderStatus.Pending;
        order.TotalAmount = 0;

        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        var response = _mapper.Map<OrderResponseDto>(order);

        return ApiResponseDto<OrderResponseDto>.SuccessResult(
            response,
            "Order created successfully.");
    }

    public async Task<ApiResponseDto<List<OrderResponseDto>>> GetAllAsync()
    {
        var orders = await _repository.GetAllAsync();

        var response = _mapper.Map<List<OrderResponseDto>>(orders);

        return ApiResponseDto<List<OrderResponseDto>>.SuccessResult(
            response,
            "Orders retrieved successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> GetByIdAsync(
        int id)
    {
        var order = await _repository.GetByIdAsync(id);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        var response = _mapper.Map<OrderResponseDto>(order);

        return ApiResponseDto<OrderResponseDto>.SuccessResult(
            response,
            "Order retrieved successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> UpdateCustomerNameAsync(
        int orderId,
        UpdateOrderCustomerNameDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Only pending orders can be updated.");
        }

        _mapper.Map(dto, order);

        order.CustomerName = string.IsNullOrWhiteSpace(order.CustomerName)
            ? null
            : order.CustomerName.Trim();

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            "Customer name updated successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status == dto.Status)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                $"Order is already in {order.Status} status.");
        }

        if (IsTerminalStatus(order.Status))
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                $"{order.Status} orders can no longer be updated.");
        }

        if (!IsAllowedTransition(order.Status, dto.Status))
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                $"Invalid status transition from {order.Status} to {dto.Status}.");
        }

        if (dto.Status == OrderStatus.Preparing &&
            order.OrderItems.Count == 0)
        {
            return ApiResponseDto<OrderResponseDto>.ErrorResult(
                "Cannot start preparing an order without items.");
        }

        order.Status = dto.Status;

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            $"Order status updated to {order.Status} successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> UpdateItemQuantityAsync(
        int orderId,
        int orderItemId,
        UpdateOrderItemQuantityDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Items can only be updated on pending orders.");
        }

        var orderItem = order.OrderItems.FirstOrDefault(
            oi => oi.Id == orderItemId);

        if (orderItem is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order item not found.");
        }

        if (dto.Quantity <= 0)
        {
            return ApiResponseDto<OrderResponseDto>.ErrorResult(
                "Quantity must be greater than zero.");
        }

        orderItem.Quantity = dto.Quantity;

        RecalculateTotal(order);

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            "Order item quantity updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);

        if (order is null)
        {
            return ApiResponseDto<bool>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<bool>.ConflictResult(
                "Only pending orders can be deleted.");
        }

        _repository.Remove(order);

        await _repository.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResult(
            true,
            "Order deleted successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> AddItemAsync(
        int orderId,
        AddOrderItemDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Items can only be added to pending orders.");
        }

        if (dto.Quantity <= 0)
        {
            return ApiResponseDto<OrderResponseDto>.ErrorResult(
                "Quantity must be greater than zero.");
        }

        var menuItem = await _repository.GetMenuItemByIdAsync(
            dto.MenuItemId);

        if (menuItem is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Menu item not found.");
        }

        if (!menuItem.IsAvailable)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Menu item is currently unavailable.");
        }

        var existingOrderItem = order.OrderItems.FirstOrDefault(
            oi => oi.MenuItemId == dto.MenuItemId);

        if (existingOrderItem is not null)
        {
            existingOrderItem.Quantity += dto.Quantity;
        }
        else
        {
            order.OrderItems.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = dto.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        RecalculateTotal(order);

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            "Item added to order successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> RemoveItemAsync(
        int orderId,
        int orderItemId)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Items can only be removed from pending orders.");
        }

        var orderItem = order.OrderItems.FirstOrDefault(
            oi => oi.Id == orderItemId);

        if (orderItem is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order item not found.");
        }

        order.OrderItems.Remove(orderItem);

        RecalculateTotal(order);

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            "Item removed from order successfully.");
    }

    public async Task<ApiResponseDto<OrderResponseDto>> CheckoutAsync(
        int orderId,
        CheckoutOrderDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ApiResponseDto<OrderResponseDto>.ConflictResult(
                "Only pending orders can be checked out.");
        }

        if (order.OrderItems.Count == 0 || order.TotalAmount <= 0)
        {
            return ApiResponseDto<OrderResponseDto>.ErrorResult(
                "Cannot checkout an order without items.");
        }

        order.PaymentMethod = dto.PaymentMethod;
        order.PaymentDate = DateTime.UtcNow;
        order.Status = OrderStatus.Preparing;

        await _repository.SaveChangesAsync();

        return await GetUpdatedOrderResponseAsync(
            order,
            "Order checked out successfully.");
    }

    private async Task<ApiResponseDto<OrderResponseDto>> GetUpdatedOrderResponseAsync(
        Order order,
        string message)
    {
        var updatedOrder = await _repository.GetByIdAsync(order.Id);

        if (updatedOrder is null)
        {
            return ApiResponseDto<OrderResponseDto>.NotFoundResult(
                "Unable to retrieve the updated order.");
        }

        var response = _mapper.Map<OrderResponseDto>(updatedOrder);

        return ApiResponseDto<OrderResponseDto>.SuccessResult(
            response,
            message);
    }

    private static bool IsTerminalStatus(OrderStatus status)
    {
        return status is OrderStatus.Completed or OrderStatus.Canceled;
    }

    private static bool IsAllowedTransition(
        OrderStatus current,
        OrderStatus next)
    {
        return (current, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Preparing) => true,
            (OrderStatus.Pending, OrderStatus.Canceled) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Preparing, OrderStatus.Canceled) => true,
            (OrderStatus.Ready, OrderStatus.Completed) => true,
            (OrderStatus.Ready, OrderStatus.Canceled) => true,
            _ => false
        };
    }

    private static void RecalculateTotal(Order order)
    {
        order.TotalAmount = order.OrderItems.Sum(
            oi => oi.Quantity * oi.UnitPrice);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }
}
