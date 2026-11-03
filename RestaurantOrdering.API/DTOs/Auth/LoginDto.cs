namespace RestaurantOrdering.API.DTOs.Auth;

/// <summary>
/// Credentials used to authenticate an internal user.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email address of the account.
    /// </summary>
    /// <example>admin@restaurant.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for the account.
    /// </summary>
    /// <example>Admin1234</example>
    public string Password { get; set; } = string.Empty;
}
