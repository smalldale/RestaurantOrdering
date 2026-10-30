using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.Models;

namespace RestaurantOrdering.Data;

/// <summary>
/// Fake data used by both modes.
///
/// The first run after the migration inserts two categories (Food, Drink) and a
/// few menu items. Every later run finds rows in <c>Categories</c> and returns
/// without inserting anything, so the seed is never duplicated.
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeededAsync(RestaurantDbContext context)
    {
        // Conditional: if the table already has data, assume the seed ran before
        // (or the user added their own) and leave it alone.
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine("Seed data already present, skipping insert.");
            return;
        }

        var food = new Category
        {
            Name = "Food",
            Description = "Main dishes and meals"
        };

        var drink = new Category
        {
            Name = "Drink",
            Description = "Beverages and refreshments"
        };

        context.Categories.AddRange(food, drink);
        await context.SaveChangesAsync();

        context.MenuItems.AddRange(
            new MenuItem
            {
                CategoryId = food.Id,
                Name = "Nasi Goreng",
                Description = "Fried rice with egg",
                Price = 25000m,
                IsAvailable = true
            },
            new MenuItem
            {
                CategoryId = food.Id,
                Name = "Mie Goreng",
                Description = "Fried noodles with vegetables",
                Price = 22000m,
                IsAvailable = true
            },
            new MenuItem
            {
                CategoryId = food.Id,
                Name = "Ayam Bakar",
                Description = "Grilled chicken with rice",
                Price = 35000m,
                IsAvailable = true
            },
            new MenuItem
            {
                CategoryId = drink.Id,
                Name = "Es Teh",
                Description = "Iced tea",
                Price = 5000m,
                IsAvailable = true
            },
            new MenuItem
            {
                CategoryId = drink.Id,
                Name = "Es Jeruk",
                Description = "Iced lime juice",
                Price = 8000m,
                IsAvailable = true
            },
            new MenuItem
            {
                CategoryId = drink.Id,
                Name = "Kopi Hitam",
                Description = "Black coffee",
                Price = 15000m,
                IsAvailable = true
            });

        await context.SaveChangesAsync();

        Console.WriteLine("Seed data inserted (2 categories, 6 menu items).");
    }
}
