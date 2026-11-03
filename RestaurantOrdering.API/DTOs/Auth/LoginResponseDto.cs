namespace RestaurantOrdering.API.DTOs.Auth;

/// <summary>
/// Result returned after a successful login or token refresh.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Signed JWT access token for authenticating subsequent requests.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Opaque refresh token used to obtain a new access token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the access token expires.
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// UTC timestamp when the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Identity user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the authenticated user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name of the authenticated user.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the authenticated user.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Roles assigned to the authenticated user, such as <c>Admin</c> or <c>Staff</c>.
    /// </summary>
    public List<string> Roles { get; set; } = [];
}
