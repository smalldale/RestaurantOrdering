using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.API.Data;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;

namespace RestaurantOrdering.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly RestaurantDbContext _context;

    public CategoryRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Category> Items, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null)
    {
        var query = _context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var skip = Math.Max(0L, (long)(pageNumber - 1) * pageSize);

        if (skip > int.MaxValue)
        {
            return (new List<Category>(), totalCount);
        }

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((int)skip)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> HasMenuItemsAsync(int categoryId)
    {
        return await _context.MenuItems
            .AnyAsync(m => m.CategoryId == categoryId);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}