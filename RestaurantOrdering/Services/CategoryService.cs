
using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.Data;
using RestaurantOrdering.Models;

namespace RestaurantOrdering.Services;

public class CategoryService
{
    private readonly RestaurantDbContext _context;

    public CategoryService(RestaurantDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    // READ: Get all categories
    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    // READ: Get category by ID
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // UPDATE
    public async Task<Category?> UpdateAsync(
        int id,
        Category updatedCategory)
    {
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCategory is null)
            return null;

        existingCategory.Name = updatedCategory.Name;
        existingCategory.Description = updatedCategory.Description;

        await _context.SaveChangesAsync();

        return existingCategory;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return false;

        bool hasMenuItems = await _context.MenuItems
            .AnyAsync(m => m.CategoryId == id);

        if (hasMenuItems)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }
}
