namespace RestaurantOrdering.API.DTOs.Auth;

/// <summary>
/// Refresh token supplied to the refresh or logout endpoint.
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// The refresh token to exchange or revoke.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
