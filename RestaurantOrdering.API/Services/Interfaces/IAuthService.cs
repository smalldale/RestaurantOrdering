using RestaurantOrdering.API.DTOs.Auth;
using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);

    Task<ApiResponseDto<LoginResponseDto>> RefreshAsync(RefreshTokenDto dto);

    Task<ApiResponseDto<bool>> LogoutAsync(RefreshTokenDto dto);
}