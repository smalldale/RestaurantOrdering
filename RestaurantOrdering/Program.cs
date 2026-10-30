using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.Data;
using RestaurantOrdering.Services;

Console.WriteLine("=== Restaurant Ordering Core Demo Application ===\n");

// db option
var options = new DbContextOptionsBuilder<RestaurantDbContext>()
    .UseSqlite("Data Source=RestaurantDatabase.db")
    .EnableSensitiveDataLogging()
    .Options;

// Initialize our database context
// In a real application, this would be configured through dependency injection
using var context = new RestaurantDbContext(options);
await ApplyMigrationsAsync(context);

// Initialize our services
var categoryService = new CategoryService(context);
var menuService = new MenuService(context);
var orderService = new OrderService(context);


/// <summary>
/// Applies pending migrations to the database
/// This is the proper way to handle database schema changes in production
/// </summary>
static async Task ApplyMigrationsAsync(CompanyDbContext context)
{
    Console.WriteLine("Applying database migrations...");
    
    // Apply any pending migrations
    await context.Database.MigrateAsync();
    
    Console.WriteLine("Database migrations applied successfully!\n");
}

// Console.WriteLine("RestaurantOrdering is ready. Apply the schema with 'dotnet ef database update'.");
