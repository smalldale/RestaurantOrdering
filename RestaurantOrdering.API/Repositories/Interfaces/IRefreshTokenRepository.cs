using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    Task<List<RefreshToken>> GetActiveByUserIdAsync(string userId);

    Task AddAsync(RefreshToken refreshToken);

    Task SaveChangesAsync();
}