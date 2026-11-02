using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Data.Seeders;

/// <summary>
/// Fake restaurant data used by the application.
/// The first run inserts two categories and six menu items.
/// Subsequent runs skip seeding if categories already exist.
/// </summary>
public static class RestaurantDataSeeder
{
    public static async Task EnsureSeededAsync(
        RestaurantDbContext context)
    {
        // If categories already exist, skip seeding.
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine(
                "Seed data already present, skipping insert.");

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

        // Save first to generate category IDs.
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

        Console.WriteLine(
            "Seed data inserted (2 categories, 6 menu items).");
    }
}
