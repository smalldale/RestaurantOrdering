using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.MenuItem;

namespace RestaurantOrdering.API.Services.Interfaces;

public interface IMenuService
{
    Task<ApiResponseDto<MenuItemResponseDto>> CreateAsync(
        CreateMenuItemDto dto);

    Task<ApiResponseDto<PagedResponseDto<MenuItemResponseDto>>> GetAllAsync(
        MenuItemQueryDto query);

    Task<ApiResponseDto<MenuItemResponseDto>> GetByIdAsync(int id);

    Task<ApiResponseDto<MenuItemResponseDto>> UpdateAsync(
        int id,
        UpdateMenuItemDto dto);

    Task<ApiResponseDto<bool>> DeleteAsync(int id);
}