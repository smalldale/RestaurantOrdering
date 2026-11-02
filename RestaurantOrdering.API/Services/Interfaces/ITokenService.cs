using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);

    (string Token, DateTime ExpiresAt) GenerateRefreshToken();

    string HashRefreshToken(string token);
}
