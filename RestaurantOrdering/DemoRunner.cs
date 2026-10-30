using RestaurantOrdering.Data;
using RestaurantOrdering.Models;
using RestaurantOrdering.Models.Enums;
using RestaurantOrdering.Services;

namespace RestaurantOrdering;

/// <summary>
/// ONE-SHOT SCENARIO (straight-line mode).
///
/// Runs the whole restaurant flow once, in a fixed order, then exits:
///
///   read seed data -> create category -> create menu item -> update menu item
///   -> create order -> add items -> read detail -> update customer
///   -> remove item -> checkout -> list orders -> delete records.
///
/// Every step prints its result, so this doubles as a smoke test for the
/// services. It is safe to run repeatedly: categories and menu items that
/// already exist are reused instead of duplicated.
/// </summary>
public static class DemoRunner
{
    public static async Task RunAsync(RestaurantDbContext db)
    {
        var categoryService = new CategoryService(db);
        var menuService = new MenuService(db);
        var orderService = new OrderService(db);

        Console.WriteLine();
        Console.WriteLine("=== ONE-SHOT SCENARIO: END-TO-END WALKTHROUGH ===");

        // STEP 1 - READ ------------------------------------------------------
        var categories = await categoryService.GetAllAsync();
        var menuItems = await menuService.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("STEP 1: read seeded categories and menu items");
        PrintCategories(categories);
        PrintMenuItems(menuItems);

        if (categories.Count == 0 || menuItems.Count == 0)
        {
            Console.WriteLine("Seed data is missing, stopping.");
            return;
        }

        var nasiGoreng = menuItems.FirstOrDefault(m => m.Name == "Nasi Goreng") ?? menuItems[0];
        var esTeh = menuItems.FirstOrDefault(m => m.Name == "Es Teh") ?? menuItems[^1];

        // STEP 2 - CREATE category -------------------------------------------
        var dessert = (await categoryService.GetAllAsync())
            .FirstOrDefault(c => c.Name == "Dessert");

        Console.WriteLine();
        Console.WriteLine("STEP 2: create category");

        if (dessert is null)
        {
            dessert = await categoryService.CreateAsync(new Category
            {
                Name = "Dessert",
                Description = "Sweet treats"
            });

            Console.WriteLine($"Created category #{dessert.Id} 'Dessert'.");
        }
        else
        {
            Console.WriteLine($"Category #{dessert.Id} 'Dessert' already exists.");
        }

        // STEP 3 - CREATE menu item ------------------------------------------
        var pudding = (await menuService.GetByCategoryAsync(dessert.Id))
            .FirstOrDefault(m => m.Name == "Puding Coklat");

        Console.WriteLine();
        Console.WriteLine("STEP 3: create menu item");

        if (pudding is null)
        {
            pudding = await menuService.CreateAsync(new MenuItem
            {
                CategoryId = dessert.Id,
                Name = "Puding Coklat",
                Description = "Chocolate pudding",
                Price = 18000m,
                IsAvailable = true
            });

            Console.WriteLine($"Created menu item #{pudding.Id} 'Puding Coklat'.");
        }
        else
        {
            Console.WriteLine($"Menu item #{pudding.Id} 'Puding Coklat' already exists.");
        }

        // STEP 4 - UPDATE menu item ------------------------------------------
        var oldPrice = pudding.Price;
        var newPrice = oldPrice + 2000m;

        var updated = await menuService.UpdateAsync(pudding.Id, new MenuItem
        {
            CategoryId = dessert.Id,
            Name = pudding.Name,
            Description = pudding.Description,
            Price = newPrice,
            IsAvailable = pudding.IsAvailable
        });

        Console.WriteLine();
        Console.WriteLine("STEP 4: update menu item price");
        Console.WriteLine(updated == true
            ? $"Price changed from {oldPrice:N2} to {pudding.Price:N2}."
            : "Menu item update failed.");

        // STEP 5 - CREATE order ----------------------------------------------
        var order = await orderService.CreateAsync("Demo Guest");

        Console.WriteLine();
        Console.WriteLine("STEP 5: create order");
        Console.WriteLine($"Created order {order.OrderNumber} (#{order.Id}).");

        // STEP 6 - CREATE order items ----------------------------------------
        var addFood = await orderService.AddItemAsync(order.Id, nasiGoreng.Id, 2);
        var addDrink = await orderService.AddItemAsync(order.Id, esTeh.Id, 1);

        Console.WriteLine();
        Console.WriteLine("STEP 6: add items to the order");
        Console.WriteLine($"Add 2x '{nasiGoreng.Name}': {addFood}");
        Console.WriteLine($"Add 1x '{esTeh.Name}': {addDrink}");

        // STEP 7 - READ order detail -----------------------------------------
        var detail = await orderService.GetByIdAsync(order.Id);

        Console.WriteLine();
        Console.WriteLine("STEP 7: read order detail");

        if (detail is not null)
            PrintOrder(detail);

        // STEP 8 - UPDATE customer name --------------------------------------
        var renamed = await orderService.UpdateCustomerNameAsync(order.Id, "Demo Guest (VIP)");

        Console.WriteLine();
        Console.WriteLine($"STEP 8: update customer name {order.CustomerName}");
        Console.WriteLine($"Result: {renamed}");

        // STEP 9 - DELETE one order item -------------------------------------
        var current = await orderService.GetByIdAsync(order.Id);

        // Remove the drink line on purpose, so the food line stays behind and
        // still blocks menu item deletion in STEP 14.
        var drinkItem = current?.OrderItems.FirstOrDefault(oi => oi.MenuItemId == esTeh.Id);

        Console.WriteLine();
        Console.WriteLine("STEP 9: remove one order item");

        if (drinkItem is null)
        {
            Console.WriteLine("No drink line on the order, nothing to remove.");
        }
        else
        {
            var removed = await orderService.RemoveItemAsync(order.Id, drinkItem.Id);
            Console.WriteLine($"Removed order item #{drinkItem.Id} ('{esTeh.Name}'): {removed}");
        }

        // STEP 10 - UPDATE checkout ------------------------------------------
        var checkout = await orderService.CheckoutAsync(order.Id, PaymentMethod.Cash);
        var checkedOut = await orderService.GetByIdAsync(order.Id);

        Console.WriteLine();
        Console.WriteLine("STEP 10: checkout");
        Console.WriteLine($"Checkout result: {checkout}");

        if (checkedOut is not null)
            PrintOrder(checkedOut);

        // STEP 11 - READ all orders ------------------------------------------
        Console.WriteLine();
        Console.WriteLine("STEP 11: list all orders");
        PrintOrders(await orderService.GetAllAsync());

        // STEP 12 - DELETE pending order -------------------------------------
        Console.WriteLine("STEP 12: delete a pending order");
        var throwaway = await orderService.CreateAsync("Throwaway Order");
        Console.WriteLine($"Created order #{throwaway.Id}");
        PrintOrders(await orderService.GetAllAsync());
        
        var deletedOrder = await orderService.DeleteAsync(throwaway.Id);
        Console.WriteLine();
        Console.WriteLine($"Deleted order #{throwaway.Id}: {deletedOrder}");
        PrintOrders(await orderService.GetAllAsync());

        // STEP 13 - DELETE empty category ------------------------------------
        var temporary = await categoryService.CreateAsync(new Category
        {
            Name = "Temporary",
            Description = "Deleted immediately"
        });

        var deletedCategory = await categoryService.DeleteAsync(temporary.Id);

        Console.WriteLine();
        Console.WriteLine("STEP 13: delete a category without menu items");
        Console.WriteLine($"Created and deleted category #{temporary.Id}: " +
                          $"{(deletedCategory ? "Success" : "Rejected")}");

        // STEP 14 - business rule: item with order history cannot be deleted --
        var deletedItem = await menuService.DeleteAsync(nasiGoreng.Id);

        Console.WriteLine();
        Console.WriteLine("STEP 14: deleting an item that has order history");
        Console.WriteLine($"Deleting '{nasiGoreng.Name}' -> " +
                          $"{(deletedItem ? "Success (unexpected)" : "Rejected (expected)")}");

        Console.WriteLine();
        Console.WriteLine("=== SCENARIO COMPLETE ===");
    }

    // ---------------------------------------------------------------------
    // Output
    // ---------------------------------------------------------------------

    private static void PrintCategories(IReadOnlyList<Category> categories)
    {
        if (categories.Count == 0)
        {
            Console.WriteLine("(no categories)");
            return;
        }

        foreach (var category in categories)
            Console.WriteLine($"  #{category.Id}  {category.Name}  {category.Description ?? "-"}");
    }

    private static void PrintMenuItems(IReadOnlyList<MenuItem> items)
    {
        if (items.Count == 0)
        {
            Console.WriteLine("(no menu items)");
            return;
        }

        foreach (var item in items)
        {
            Console.WriteLine(
                $"  #{item.Id}  {item.Name}  [{item.Category?.Name ?? $"#{item.CategoryId}"}]  " +
                $"{item.Price:N2}  {(item.IsAvailable ? "available" : "unavailable")}");
        }
    }

    private static void PrintOrders(IReadOnlyList<Order> orders)
    {
        if (orders.Count == 0)
        {
            Console.WriteLine("(no orders)");
            return;
        }

        foreach (var order in orders)
        {
            Console.WriteLine(
                $"  #{order.Id}  {order.OrderNumber}  {order.CustomerName ?? "-"}  " +
                $"{order.Status}  {order.TotalAmount:N2}  " +
                $"{order.OrderDate.ToLocalTime():yyyy-MM-dd HH:mm}");
        }
    }

    private static void PrintOrder(Order order)
    {
        Console.WriteLine($"Order {order.OrderNumber} (#{order.Id})");
        Console.WriteLine($"  Customer : {order.CustomerName ?? "-"}");
        Console.WriteLine($"  Status   : {order.Status}");
        Console.WriteLine($"  Payment  : {order.PaymentMethod?.ToString() ?? "-"}");

        foreach (var item in order.OrderItems)
        {
            Console.WriteLine(
                $"  #{item.Id}  {item.MenuItem?.Name ?? $"#{item.MenuItemId}"}  " +
                $"x{item.Quantity}  {item.UnitPrice:N2}  = {item.Quantity * item.UnitPrice:N2}");
        }

        Console.WriteLine($"  Total    : {order.TotalAmount:N2}");
    }
}
