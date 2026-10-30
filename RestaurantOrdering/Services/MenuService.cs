
using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.Data;
using RestaurantOrdering.Models;

namespace RestaurantOrdering.Services;

public class MenuService
{
    private readonly RestaurantDbContext _context;

    public MenuService(RestaurantDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<MenuItem> CreateAsync(MenuItem menuItem)
    {
        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == menuItem.CategoryId);

        if (!categoryExists)
            throw new ArgumentException("Category does not exist.");

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

    // READ: Get all menu items
    public async Task<List<MenuItem>> GetAllAsync()
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    // READ: Get available menu items
    public async Task<List<MenuItem>> GetAvailableAsync()
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.IsAvailable)
            .AsNoTracking()
            .ToListAsync();
    }

    // READ: Get menu item by ID
    public async Task<MenuItem?> GetByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // READ: Get menu items by category
    public async Task<List<MenuItem>> GetByCategoryAsync(int categoryId)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync();
    }

    // UPDATE
    public async Task<bool?> UpdateAsync(int id, MenuItem updatedItem)
    {
        var existingMenuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existingMenuItem is null)
            return null;

        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == updatedItem.CategoryId);

        if (!categoryExists)
            return false;

        existingMenuItem.Name = updatedItem.Name;
        existingMenuItem.Description = updatedItem.Description;
        existingMenuItem.CategoryId = updatedItem.CategoryId;
        existingMenuItem.Price = updatedItem.Price;
        existingMenuItem.IsAvailable = updatedItem.IsAvailable;

        await _context.SaveChangesAsync();

        return true;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem is null)
            return false;

        bool hasOrderHistory = await _context.OrderItems
            .AnyAsync(oi => oi.MenuItemId == id);

        if (hasOrderHistory)
            return false;

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();

        return true;
    }
}
