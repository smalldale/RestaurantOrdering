using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);

    Task AddAsync(Order order);
    void Remove(Order order);

    Task SaveChangesAsync();
}
