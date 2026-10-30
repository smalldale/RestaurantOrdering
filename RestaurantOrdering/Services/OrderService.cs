
using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.Data;
using RestaurantOrdering.Models;
using RestaurantOrdering.Models.Enums;
using RestaurantOrdering.Services.Enums;

namespace RestaurantOrdering.Services;

public class OrderService
{
    private readonly RestaurantDbContext _context;

    public OrderService(RestaurantDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<Order> CreateAsync(string? customerName = null)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerName = string.IsNullOrWhiteSpace(customerName)
                ? null
                : customerName.Trim(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    // READ: Get all orders
    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // READ: Get order by ID
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    // UPDATE: Customer name
    public async Task<OrderOperationResult> UpdateCustomerNameAsync(
        int orderId,
        string? customerName)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return OrderOperationResult.OrderNotFound;

        if (order.Status != OrderStatus.Pending)
            return OrderOperationResult.OrderNotPending;

        order.CustomerName = string.IsNullOrWhiteSpace(customerName)
            ? null
            : customerName.Trim();

        await _context.SaveChangesAsync();

        return OrderOperationResult.Success;
    }

    // DELETE
    public async Task<OrderOperationResult> DeleteAsync(int id)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return OrderOperationResult.OrderNotFound;

        if (order.Status != OrderStatus.Pending)
            return OrderOperationResult.OrderNotPending;

        _context.Orders.Remove(order);

        await _context.SaveChangesAsync();

        return OrderOperationResult.Success;
    }

    // CREATE: Add menu item to order
    public async Task<OrderOperationResult> AddItemAsync(
        int orderId,
        int menuItemId,
        int quantity)
    {
        if (quantity <= 0)
            return OrderOperationResult.InvalidQuantity;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return OrderOperationResult.OrderNotFound;

        if (order.Status != OrderStatus.Pending)
            return OrderOperationResult.OrderNotPending;

        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem is null)
            return OrderOperationResult.MenuItemNotFound;

        if (!menuItem.IsAvailable)
            return OrderOperationResult.MenuItemUnavailable;

        var existingOrderItem = order.OrderItems
            .FirstOrDefault(oi => oi.MenuItemId == menuItemId);

        if (existingOrderItem is not null)
        {
            existingOrderItem.Quantity += quantity;
        }
        else
        {
            var orderItem = new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = quantity,
                UnitPrice = menuItem.Price
            };

            order.OrderItems.Add(orderItem);
        }

        RecalculateTotal(order);

        await _context.SaveChangesAsync();

        return OrderOperationResult.Success;
    }

    // DELETE: Remove menu item from order
    public async Task<OrderOperationResult> RemoveItemAsync(
        int orderId,
        int orderItemId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return OrderOperationResult.OrderNotFound;

        if (order.Status != OrderStatus.Pending)
            return OrderOperationResult.OrderNotPending;

        var orderItem = order.OrderItems
            .FirstOrDefault(oi => oi.Id == orderItemId);

        if (orderItem is null)
            return OrderOperationResult.OrderItemNotFound;

        order.OrderItems.Remove(orderItem);

        RecalculateTotal(order);

        await _context.SaveChangesAsync();

        return OrderOperationResult.Success;
    }

    // CHECKOUT
    public async Task<OrderOperationResult> CheckoutAsync(
        int orderId,
        PaymentMethod paymentMethod)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return OrderOperationResult.OrderNotFound;

        if (order.Status != OrderStatus.Pending)
            return OrderOperationResult.OrderNotPending;

        if (order.OrderItems.Count == 0 || order.TotalAmount <= 0)
            return OrderOperationResult.OrderHasNoItems;

        order.PaymentMethod = paymentMethod;
        order.PaymentDate = DateTime.UtcNow;
        order.Status = OrderStatus.Preparing;

        await _context.SaveChangesAsync();

        return OrderOperationResult.Success;
    }

    // CALCULATE: Recalculate order total
    private static void RecalculateTotal(Order order)
    {
        order.TotalAmount = order.OrderItems
            .Sum(oi => oi.Quantity * oi.UnitPrice);
    }

    // GENERATE: Unique order number
    private static string GenerateOrderNumber()
    {
        return $"ORD-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }
}
