using Microsoft.EntityFrameworkCore;
using RestaurantOrdering;
using RestaurantOrdering.Data;

// ============================================================================
// RestaurantOrdering - console entry point
// ----------------------------------------------------------------------------
// Two ways to drive the same services:
//
//   1. DEMO (one-shot scenario)
//      A straight-line walkthrough: seed data, CRUD for every entity, add and
//      remove order items, checkout, then exit. Nothing to type.
//
//   2. INTERACTIVE (menu driven)
//      A looping console CRUD front end for Categories, Menu items and Orders.
//
// Pick with an argument:
//   dotnet run -- demo
//   dotnet run -- interactive
//   dotnet run                 (asks which mode)
// ============================================================================

var options = new DbContextOptionsBuilder<RestaurantDbContext>()
    .UseSqlite("Data Source=restaurant.db")
    .EnableSensitiveDataLogging()
    .Options;

await using var context = new RestaurantDbContext(options);

// 1) Apply migrations so the schema matches the model.
// 2) Insert the fake Category (Food, Drink) + MenuItem rows once. The seed
//    checks the table first, so a second run does not duplicate anything.
Console.WriteLine("Applying database migrations...");
await context.Database.MigrateAsync();
await SeedData.EnsureSeededAsync(context);

var mode = args.FirstOrDefault()?.Trim().ToLowerInvariant();

if (string.IsNullOrEmpty(mode))
{
    Console.WriteLine();
    Console.WriteLine("Choose a mode:");
    Console.WriteLine("  1. Interactive console CRUD");
    Console.WriteLine("  2. One-shot scenario (straight-line)");
    Console.Write("Select [1/2]: ");

    mode = Console.ReadLine()?.Trim() switch
    {
        "2" => "demo",
        _ => "interactive"
    };
}

try
{
    switch (mode)
    {
        case "demo":
            await DemoRunner.RunAsync(context);
            break;

        case "interactive":
            await InteractiveMenu.RunAsync(context);
            break;

        default:
            Console.WriteLine($"Unknown mode '{mode}'. Use: dotnet run -- demo | interactive");
            return 1;
    }
}
catch (EndOfStreamException)
{
    // stdin closed (Ctrl+Z / piped input finished): leave quietly.
    Console.WriteLine();
    Console.WriteLine("Input closed. Goodbye.");
}

Console.WriteLine();
Console.WriteLine("Done.");
return 0;
