using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<(List<Category> Items, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null);

    Task<Category?> GetByIdAsync(int id);

    Task<bool> HasMenuItemsAsync(int categoryId);

    Task AddAsync(Category category);

    void Remove(Category category);

    Task SaveChangesAsync();
}