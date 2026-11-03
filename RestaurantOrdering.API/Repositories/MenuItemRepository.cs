using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.API.Data;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;

namespace RestaurantOrdering.API.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly RestaurantDbContext _context;

    public MenuItemRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<(List<MenuItem> Items, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? categoryId = null,
        bool? isAvailable = null)
    {
        var query = _context.MenuItems
            .Include(m => m.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m =>
                m.Name.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m =>
                m.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(m =>
                m.IsAvailable == isAvailable.Value);
        }

        var totalCount = await query.CountAsync();

        var skip = Math.Max(0L, (long)(pageNumber - 1) * pageSize);

        if (skip > int.MaxValue)
        {
            return (new List<MenuItem>(), totalCount);
        }

        var items = await query
            .OrderBy(m => m.Id)
            .Skip((int)skip)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<MenuItem?> GetByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == categoryId);
    }

    public async Task<bool> HasOrderItemsAsync(int menuItemId)
    {
        return await _context.OrderItems
            .AnyAsync(oi => oi.MenuItemId == menuItemId);
    }

    public async Task AddAsync(MenuItem menuItem)
    {
        await _context.MenuItems.AddAsync(menuItem);
    }

    public void Remove(MenuItem menuItem)
    {
        _context.MenuItems.Remove(menuItem);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
