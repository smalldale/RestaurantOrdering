using Microsoft.AspNetCore.Identity;
using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Data.Seeders;

/// <summary>
/// Seeds authentication-related data:
/// Admin and Staff roles, and the initial Admin account.
/// </summary>
public static class IdentitySeeder
{
    public static async Task EnsureSeededAsync(
        IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        string[] roles = ["Admin", "Staff"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Get initial Admin credentials from environment variables
        var adminEmail = Environment.GetEnvironmentVariable(
            "INITIAL_ADMIN_EMAIL");

        var adminPassword = Environment.GetEnvironmentVariable(
            "INITIAL_ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        // Check whether the Admin account already exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(
                adminUser, adminPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create initial Admin: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Ensure the Admin account has the Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                adminUser, "Admin");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}