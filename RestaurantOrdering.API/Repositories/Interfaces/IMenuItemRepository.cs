using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Repositories.Interfaces;

public interface IMenuItemRepository
{
    Task<(List<MenuItem> Items, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? categoryId = null,
        bool? isAvailable = null);
    Task<MenuItem?> GetByIdAsync(int id);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> HasOrderItemsAsync(int menuItemId);

    Task AddAsync(MenuItem menuItem);
    void Remove(MenuItem menuItem);

    Task SaveChangesAsync();
}
