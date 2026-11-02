using Microsoft.AspNetCore.Identity;
using RestaurantOrdering.API.DTOs.Auth;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || !user.IsActive)
        {
            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Invalid email or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken.Token),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshToken.ExpiresAt
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };

        return ApiResponseDto<LoginResponseDto>.SuccessResult(
            response,
            "Login successful.");
    }

    public async Task<ApiResponseDto<LoginResponseDto>> RefreshAsync(
    RefreshTokenDto dto)
    {
        var tokenHash = _tokenService.HashRefreshToken(dto.RefreshToken);

        var existingToken = await _refreshTokenRepository
            .GetByTokenHashAsync(tokenHash);

        if (existingToken is null)
        {
            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Invalid refresh token.");
        }

        // Detect reuse of a previously rotated refresh token.
        if (existingToken.RevokedAt.HasValue)
        {
            if (existingToken.ReplacedByTokenHash is not null)
            {
                var activeTokens = await _refreshTokenRepository
                    .GetActiveByUserIdAsync(existingToken.UserId);

                foreach (var activeToken in activeTokens)
                {
                    activeToken.RevokedAt = DateTime.UtcNow;
                }

                await _refreshTokenRepository.SaveChangesAsync();
            }

            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Invalid refresh token.");
        }

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Refresh token has expired.");
        }

        var user = existingToken.User;

        if (!user.IsActive)
        {
            existingToken.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.SaveChangesAsync();

            return ApiResponseDto<LoginResponseDto>.ErrorResult(
                "Invalid refresh token.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var newRefreshTokenHash = _tokenService.HashRefreshToken(
            refreshToken.Token);

        // Revoke the old token and link it to its replacement.
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = newRefreshTokenHash;

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshToken.ExpiresAt
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };

        return ApiResponseDto<LoginResponseDto>.SuccessResult(
            response,
            "Token refreshed successfully.");
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(RefreshTokenDto dto)
    {
        var tokenHash = _tokenService.HashRefreshToken(dto.RefreshToken);

        var refreshToken = await _refreshTokenRepository
            .GetByTokenHashAsync(tokenHash);

        // Return success even if the token does not exist or is already revoked.
        // This makes logout effectively idempotent.
        if (refreshToken is null || refreshToken.RevokedAt.HasValue)
        {
            return ApiResponseDto<bool>.SuccessResult(
                true,
                "Logout successful.");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;

        await _refreshTokenRepository.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResult(
            true,
            "Logout successful.");
    }
}