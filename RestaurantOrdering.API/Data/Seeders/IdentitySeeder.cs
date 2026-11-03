using Microsoft.AspNetCore.Identity;
using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Data.Seeders;

/// <summary>
/// Seeds authentication-related data:
/// Admin and Staff roles, plus the initial Admin and Staff accounts.
/// </summary>
public static class IdentitySeeder
{
    private const string AdminEmail = "admin@restaurant.com";
    private const string AdminPassword = "Admin1234";

    private const string StaffEmail = "staff@restaurant.com";
    private const string StaffPassword = "Staff1234";

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

        // Seed the initial Admin account
        await EnsureUserAsync(
            userManager,
            AdminEmail,
            AdminPassword,
            "System",
            "Administrator",
            "Admin");

        // Seed the initial Staff account
        await EnsureUserAsync(
            userManager,
            StaffEmail,
            StaffPassword,
            "Restaurant",
            "Staff",
            "Staff");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user '{email}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Ensure the account has the expected role
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign role '{role}' to '{email}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
