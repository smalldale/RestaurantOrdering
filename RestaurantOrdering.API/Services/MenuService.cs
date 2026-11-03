using AutoMapper;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Services;

public class MenuService : IMenuService
{
    private readonly IMenuItemRepository _repository;
    private readonly IMapper _mapper;

    public MenuService(
        IMenuItemRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<MenuItemResponseDto>> CreateAsync(
        CreateMenuItemDto dto)
    {
        bool categoryExists = await _repository.CategoryExistsAsync(
            dto.CategoryId);

        if (!categoryExists)
        {
            return ApiResponseDto<MenuItemResponseDto>.NotFoundResult(
                "Category not found.");
        }

        var menuItem = _mapper.Map<MenuItem>(dto);

        await _repository.AddAsync(menuItem);
        await _repository.SaveChangesAsync();

        var createdMenuItem = await _repository.GetByIdAsync(menuItem.Id);

        var response = _mapper.Map<MenuItemResponseDto>(createdMenuItem);

        return ApiResponseDto<MenuItemResponseDto>.SuccessResult(
            response,
            "Menu item created successfully.");
    }

    public async Task<ApiResponseDto<PagedResponseDto<MenuItemResponseDto>>> GetAllAsync(
        MenuItemQueryDto query)
    {
        var (menuItems, totalCount) = await _repository.GetAllAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            query.CategoryId,
            query.IsAvailable);

        var responseItems = _mapper.Map<List<MenuItemResponseDto>>(menuItems);

        var response = new PagedResponseDto<MenuItemResponseDto>
        {
            Items = responseItems,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = query.PageSize > 0
                ? (int)Math.Ceiling(
                    (double)totalCount / query.PageSize)
                : 0
        };

        return ApiResponseDto<PagedResponseDto<MenuItemResponseDto>>.SuccessResult(
            response,
            "Menu items retrieved successfully.");
    }

    public async Task<ApiResponseDto<MenuItemResponseDto>> GetByIdAsync(
        int id)
    {
        var menuItem = await _repository.GetByIdAsync(id);

        if (menuItem is null)
        {
            return ApiResponseDto<MenuItemResponseDto>.NotFoundResult(
                "Menu item not found.");
        }

        var response = _mapper.Map<MenuItemResponseDto>(menuItem);

        return ApiResponseDto<MenuItemResponseDto>.SuccessResult(
            response,
            "Menu item retrieved successfully.");
    }

    public async Task<ApiResponseDto<MenuItemResponseDto>> UpdateAsync(
        int id,
        UpdateMenuItemDto dto)
    {
        var menuItem = await _repository.GetByIdAsync(id);

        if (menuItem is null)
        {
            return ApiResponseDto<MenuItemResponseDto>.NotFoundResult(
                "Menu item not found.");
        }

        bool categoryExists = await _repository.CategoryExistsAsync(
            dto.CategoryId);

        if (!categoryExists)
        {
            return ApiResponseDto<MenuItemResponseDto>.NotFoundResult(
                "Category not found.");
        }

        _mapper.Map(dto, menuItem);

        await _repository.SaveChangesAsync();

        // Reload the menu item to get its updated Category navigation property.
        var updatedMenuItem = await _repository.GetByIdAsync(id);

        var response = _mapper.Map<MenuItemResponseDto>(updatedMenuItem);

        return ApiResponseDto<MenuItemResponseDto>.SuccessResult(
            response,
            "Menu item updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var menuItem = await _repository.GetByIdAsync(id);

        if (menuItem is null)
        {
            return ApiResponseDto<bool>.NotFoundResult(
                "Menu item not found.");
        }

        bool hasOrderHistory = await _repository.HasOrderItemsAsync(id);

        if (hasOrderHistory)
        {
            return ApiResponseDto<bool>.ConflictResult(
                "Cannot delete a menu item that has order history.");
        }

        _repository.Remove(menuItem);

        await _repository.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResult(
            true,
            "Menu item deleted successfully.");
    }
}