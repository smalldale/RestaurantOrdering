using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponseDto<CategoryResponseDto>> CreateAsync(
        CreateCategoryDto dto);

    Task<ApiResponseDto<PagedResponseDto<CategoryResponseDto>>> GetAllAsync(
        CategoryQueryDto query);

    Task<ApiResponseDto<CategoryResponseDto>> GetByIdAsync(int id);

    Task<ApiResponseDto<CategoryResponseDto>> UpdateAsync(
        int id,
        UpdateCategoryDto dto);

    Task<ApiResponseDto<bool>> DeleteAsync(int id);
}