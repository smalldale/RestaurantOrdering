using RestaurantOrdering.Data;
using RestaurantOrdering.Models;
using RestaurantOrdering.Models.Enums;
using RestaurantOrdering.Services;
using RestaurantOrdering.Services.Enums;

namespace RestaurantOrdering;

/// <summary>
/// INTERACTIVE MODE.
///
/// A simple menu driven console front end for the services:
/// Categories, Menu items, and Orders (add/remove order items, checkout).
/// One RestaurantDbContext is shared for the whole session; a real web app
/// would create one per request instead.
/// </summary>
public static class InteractiveMenu
{
    public static async Task RunAsync(RestaurantDbContext db)
    {
        var categories = new CategoryService(db);
        var menu = new MenuService(db);
        var orders = new OrderService(db);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== MAIN MENU ===");
            Console.WriteLine("1. Categories");
            Console.WriteLine("2. Menu items");
            Console.WriteLine("3. Orders");
            Console.WriteLine("0. Exit");

            var choice = ReadChoice("Select", 0, 3);
            if (choice == 0)
                return;

            switch (choice)
            {
                case 1:
                    await CategoryMenuAsync(categories);
                    break;
                case 2:
                    await MenuItemMenuAsync(categories, menu);
                    break;
                case 3:
                    await OrderMenuAsync(menu, orders);
                    break;
            }
        }
    }

    // =====================================================================
    // Categories
    // =====================================================================

    private static async Task CategoryMenuAsync(CategoryService categories)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== CATEGORIES ===");
            Console.WriteLine("1. List");
            Console.WriteLine("2. Add");
            Console.WriteLine("3. Update");
            Console.WriteLine("4. Delete");
            Console.WriteLine("0. Back");

            var choice = ReadChoice("Select", 0, 4);
            if (choice == 0)
                return;

            try
            {
                switch (choice)
                {
                    case 1:
                        PrintCategories(await categories.GetAllAsync());
                        break;

                    case 2:
                    {
                        var name = ReadRequired("Name");
                        var description = ReadString("Description (optional)");

                        var created = await categories.CreateAsync(new Category
                        {
                            Name = name,
                            Description = description.Length == 0 ? null : description
                        });

                        Console.WriteLine($"Created category #{created.Id} '{created.Name}'.");
                        break;
                    }

                    case 3:
                    {
                        var id = ReadInt("Category id");
                        var existing = await categories.GetByIdAsync(id);

                        if (existing is null)
                        {
                            Console.WriteLine($"Category #{id} not found.");
                            break;
                        }

                        var name = ReadString("Name", existing.Name);
                        var description = ReadString("Description", existing.Description);

                        var updated = await categories.UpdateAsync(id, new Category
                        {
                            Name = name,
                            Description = description.Length == 0 ? null : description
                        });

                        Console.WriteLine(updated is null
                            ? $"Category #{id} not found."
                            : $"Updated category #{updated.Id} '{updated.Name}'.");
                        break;
                    }

                    case 4:
                    {
                        var id = ReadInt("Category id");
                        var existing = await categories.GetByIdAsync(id);

                        if (existing is null)
                        {
                            Console.WriteLine($"Category #{id} not found.");
                            break;
                        }

                        if (!ReadBool($"Delete '{existing.Name}'", false))
                        {
                            Console.WriteLine("Canceled.");
                            break;
                        }

                        var deleted = await categories.DeleteAsync(id);

                        Console.WriteLine(deleted
                            ? $"Deleted category #{id}."
                            : "Cannot delete: the category still has menu items.");
                        break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Pause();
        }
    }

    // =====================================================================
    // Menu items
    // =====================================================================

    private static async Task MenuItemMenuAsync(CategoryService categories, MenuService menu)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== MENU ITEMS ===");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. List available only");
            Console.WriteLine("3. List by category");
            Console.WriteLine("4. Add");
            Console.WriteLine("5. Update");
            Console.WriteLine("6. Delete");
            Console.WriteLine("0. Back");

            var choice = ReadChoice("Select", 0, 6);
            if (choice == 0)
                return;

            try
            {
                switch (choice)
                {
                    case 1:
                        PrintMenuItems(await menu.GetAllAsync());
                        break;

                    case 2:
                        PrintMenuItems(await menu.GetAvailableAsync());
                        break;

                    case 3:
                    {
                        var categoryList = await categories.GetAllAsync();
                        PrintCategories(categoryList);

                        if (categoryList.Count == 0)
                        {
                            Console.WriteLine("No categories yet.");
                            break;
                        }

                        var categoryId = ReadInt("Category id");
                        PrintMenuItems(await menu.GetByCategoryAsync(categoryId));
                        break;
                    }

                    case 4:
                    {
                        var categoryList = await categories.GetAllAsync();
                        PrintCategories(categoryList);

                        if (categoryList.Count == 0)
                        {
                            Console.WriteLine("No categories yet. Add a category first.");
                            break;
                        }

                        var categoryId = ReadInt("Category id");
                        var name = ReadRequired("Name");
                        var description = ReadString("Description (optional)");
                        var price = ReadDecimal("Price");
                        var available = ReadBool("Available", true);

                        var created = await menu.CreateAsync(new MenuItem
                        {
                            CategoryId = categoryId,
                            Name = name,
                            Description = description.Length == 0 ? null : description,
                            Price = price,
                            IsAvailable = available
                        });

                        Console.WriteLine($"Created menu item #{created.Id} '{created.Name}'.");
                        break;
                    }

                    case 5:
                    {
                        var id = ReadInt("Menu item id");
                        var existing = await menu.GetByIdAsync(id);

                        if (existing is null)
                        {
                            Console.WriteLine($"Menu item #{id} not found.");
                            break;
                        }

                        PrintMenuItems(new[] { existing });
                        PrintCategories(await categories.GetAllAsync());

                        var categoryId = ReadInt("Category id", 1, existing.CategoryId);
                        var name = ReadString("Name", existing.Name);
                        var description = ReadString("Description", existing.Description);
                        var price = ReadDecimal("Price", existing.Price);
                        var available = ReadBool("Available", existing.IsAvailable);

                        var updated = await menu.UpdateAsync(id, new MenuItem
                        {
                            CategoryId = categoryId,
                            Name = name,
                            Description = description.Length == 0 ? null : description,
                            Price = price,
                            IsAvailable = available
                        });

                        Console.WriteLine(updated switch
                        {
                            true => $"Updated menu item #{id}.",
                            false => $"Category #{categoryId} not found.",
                            null => $"Menu item #{id} not found."
                        });
                        break;
                    }

                    case 6:
                    {
                        var id = ReadInt("Menu item id");
                        var existing = await menu.GetByIdAsync(id);

                        if (existing is null)
                        {
                            Console.WriteLine($"Menu item #{id} not found.");
                            break;
                        }

                        if (!ReadBool($"Delete '{existing.Name}'", false))
                        {
                            Console.WriteLine("Canceled.");
                            break;
                        }

                        var deleted = await menu.DeleteAsync(id);

                        Console.WriteLine(deleted
                            ? $"Deleted menu item #{id}."
                            : "Cannot delete: the item is used by existing order items.");
                        break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Pause();
        }
    }

    // =====================================================================
    // Orders
    // =====================================================================

    private static async Task OrderMenuAsync(MenuService menu, OrderService orders)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== ORDERS ===");
            Console.WriteLine("1. List orders");
            Console.WriteLine("2. View order detail");
            Console.WriteLine("3. Create order");
            Console.WriteLine("4. Update customer name");
            Console.WriteLine("5. Add item");
            Console.WriteLine("6. Remove item");
            Console.WriteLine("7. Checkout");
            Console.WriteLine("8. Delete order");
            Console.WriteLine("0. Back");

            var choice = ReadChoice("Select", 0, 8);
            if (choice == 0)
                return;

            try
            {
                switch (choice)
                {
                    case 1:
                        PrintOrders(await orders.GetAllAsync());
                        break;

                    case 2:
                    {
                        var order = await orders.GetByIdAsync(ReadInt("Order id"));

                        if (order is null)
                        {
                            Console.WriteLine("Order not found.");
                            break;
                        }

                        PrintOrder(order);
                        break;
                    }

                    case 3:
                    {
                        var customer = ReadString("Customer name (optional)");

                        var created = await orders.CreateAsync(
                            customer.Length == 0 ? null : customer);

                        Console.WriteLine($"Created order {created.OrderNumber} (#{created.Id}).");
                        PrintOrder(created);
                        break;
                    }

                    case 4:
                    {
                        var orderId = ReadInt("Order id");
                        var order = await orders.GetByIdAsync(orderId);

                        if (order is null)
                        {
                            Console.WriteLine($"Order #{orderId} not found.");
                            break;
                        }

                        var name = ReadString("Customer name", order.CustomerName);

                        PrintResult(await orders.UpdateCustomerNameAsync(
                            orderId,
                            name.Length == 0 ? null : name));
                        break;
                    }

                    case 5:
                    {
                        var orderId = ReadInt("Order id");
                        var order = await orders.GetByIdAsync(orderId);

                        if (order is null)
                        {
                            Console.WriteLine($"Order #{orderId} not found.");
                            break;
                        }

                        PrintOrder(order);
                        PrintMenuItems(await menu.GetAvailableAsync());

                        var menuItemId = ReadInt("Menu item id");
                        var quantity = ReadInt("Quantity");

                        PrintResult(await orders.AddItemAsync(orderId, menuItemId, quantity));
                        break;
                    }

                    case 6:
                    {
                        var orderId = ReadInt("Order id");
                        var order = await orders.GetByIdAsync(orderId);

                        if (order is null)
                        {
                            Console.WriteLine($"Order #{orderId} not found.");
                            break;
                        }

                        if (order.OrderItems.Count == 0)
                        {
                            Console.WriteLine("This order has no items.");
                            break;
                        }

                        PrintOrder(order);

                        var orderItemId = ReadInt("Order item id");

                        PrintResult(await orders.RemoveItemAsync(orderId, orderItemId));
                        break;
                    }

                    case 7:
                    {
                        var orderId = ReadInt("Order id");
                        var order = await orders.GetByIdAsync(orderId);

                        if (order is null)
                        {
                            Console.WriteLine($"Order #{orderId} not found.");
                            break;
                        }

                        PrintOrder(order);

                        var methods = Enum.GetValues<PaymentMethod>();
                        for (var i = 0; i < methods.Length; i++)
                            Console.WriteLine($"{i + 1}. {methods[i]}");

                        var method = methods[ReadChoice("Payment method", 1, methods.Length) - 1];
                        var result = await orders.CheckoutAsync(orderId, method);

                        PrintResult(result);

                        if (result == OrderOperationResult.Success)
                        {
                            var updated = await orders.GetByIdAsync(orderId);

                            if (updated is not null)
                                PrintOrder(updated);
                        }

                        break;
                    }

                    case 8:
                    {
                        var orderId = ReadInt("Order id");
                        var order = await orders.GetByIdAsync(orderId);

                        if (order is null)
                        {
                            Console.WriteLine($"Order #{orderId} not found.");
                            break;
                        }

                        if (!ReadBool($"Delete order {order.OrderNumber}", false))
                        {
                            Console.WriteLine("Canceled.");
                            break;
                        }

                        PrintResult(await orders.DeleteAsync(orderId));
                        break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Pause();
        }
    }

    // =====================================================================
    // Output
    // =====================================================================

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
        Console.WriteLine();
        Console.WriteLine($"Order {order.OrderNumber} (#{order.Id})");
        Console.WriteLine($"  Customer : {order.CustomerName ?? "-"}");
        Console.WriteLine($"  Status   : {order.Status}");
        Console.WriteLine($"  Created  : {order.OrderDate.ToLocalTime():yyyy-MM-dd HH:mm}");
        Console.WriteLine($"  Payment  : {order.PaymentMethod?.ToString() ?? "-"}");
        Console.WriteLine($"  Paid at  : {order.PaymentDate?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "-"}");

        if (order.OrderItems.Count == 0)
        {
            Console.WriteLine("  (no items)");
        }
        else
        {
            foreach (var item in order.OrderItems)
            {
                Console.WriteLine(
                    $"  #{item.Id}  {item.MenuItem?.Name ?? $"#{item.MenuItemId}"}  " +
                    $"x{item.Quantity}  {item.UnitPrice:N2}  = {item.Quantity * item.UnitPrice:N2}");
            }
        }

        Console.WriteLine($"  Total    : {order.TotalAmount:N2}");
    }

    private static void PrintResult(OrderOperationResult result) =>
        Console.WriteLine(result == OrderOperationResult.Success ? "OK." : $"Failed: {result}");

    private static void Pause()
    {
        Console.Write("Press Enter to continue...");
        Console.ReadLine();
    }

    // =====================================================================
    // Input
    // =====================================================================

    private static string ReadString(string label, string? current = null)
    {
        Console.Write(current is null ? $"{label}: " : $"{label} [{current}]: ");

        var input = Console.ReadLine()
            ?? throw new EndOfStreamException("Input closed.");

        if (string.IsNullOrWhiteSpace(input))
            return current ?? string.Empty;

        return input.Trim();
    }

    private static string ReadRequired(string label)
    {
        while (true)
        {
            var value = ReadString(label);

            if (value.Length > 0)
                return value;

            Console.WriteLine($"{label} is required.");
        }
    }

    private static int ReadInt(string label, int min = 1, int? current = null)
    {
        while (true)
        {
            var raw = ReadString(label, current?.ToString());

            if (int.TryParse(raw, out var value) && value >= min)
                return value;

            Console.WriteLine($"Enter a number >= {min}.");
        }
    }

    private static int ReadChoice(string label, int min, int max)
    {
        while (true)
        {
            var raw = ReadString(label);

            if (int.TryParse(raw, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Enter a number between {min} and {max}.");
        }
    }

    private static decimal ReadDecimal(string label, decimal? current = null)
    {
        while (true)
        {
            var raw = ReadString(label, current?.ToString("0.##"));

            if (decimal.TryParse(raw, out var value) && value >= 0m)
                return value;

            Console.WriteLine("Enter a number >= 0.");
        }
    }

    private static bool ReadBool(string label, bool current)
    {
        while (true)
        {
            var raw = ReadString($"{label} (y/n)", current ? "y" : "n").ToLowerInvariant();

            if (raw is "y" or "yes") return true;
            if (raw is "n" or "no") return false;

            Console.WriteLine("Please answer y or n.");
        }
    }
}
