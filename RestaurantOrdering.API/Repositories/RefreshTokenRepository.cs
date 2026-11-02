using Microsoft.EntityFrameworkCore;
using RestaurantOrdering.API.Data;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;

namespace RestaurantOrdering.API.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly RestaurantDbContext _context;

    public RefreshTokenRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}