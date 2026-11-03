using AutoMapper;
using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories.Interfaces;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<CategoryResponseDto>> CreateAsync(
        CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);

        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        var response = _mapper.Map<CategoryResponseDto>(category);

        return ApiResponseDto<CategoryResponseDto>.SuccessResult(
            response,
            "Category created successfully.");
    }

    public async Task<ApiResponseDto<PagedResponseDto<CategoryResponseDto>>> GetAllAsync(
        CategoryQueryDto query)
    {
        var (categories, totalCount) = await _repository.GetAllAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm);

        var responseItems = _mapper.Map<List<CategoryResponseDto>>(categories);

        var response = new PagedResponseDto<CategoryResponseDto>
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

        return ApiResponseDto<PagedResponseDto<CategoryResponseDto>>.SuccessResult(
            response,
            "Categories retrieved successfully.");
    }

    public async Task<ApiResponseDto<CategoryResponseDto>> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category is null)
        {
            return ApiResponseDto<CategoryResponseDto>.NotFoundResult(
                "Category not found.");
        }

        var response = _mapper.Map<CategoryResponseDto>(category);

        return ApiResponseDto<CategoryResponseDto>.SuccessResult(
            response,
            "Category retrieved successfully.");
    }

    public async Task<ApiResponseDto<CategoryResponseDto>> UpdateAsync(
        int id,
        UpdateCategoryDto dto)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category is null)
        {
            return ApiResponseDto<CategoryResponseDto>.NotFoundResult(
                "Category not found.");
        }

        _mapper.Map(dto, category);

        await _repository.SaveChangesAsync();

        var response = _mapper.Map<CategoryResponseDto>(category);

        return ApiResponseDto<CategoryResponseDto>.SuccessResult(
            response,
            "Category updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category is null)
        {
            return ApiResponseDto<bool>.NotFoundResult(
                "Category not found.");
        }

        bool hasMenuItems = await _repository.HasMenuItemsAsync(id);

        if (hasMenuItems)
        {
            return ApiResponseDto<bool>.ConflictResult(
                "Cannot delete a category that contains menu items.");
        }

        _repository.Remove(category);

        await _repository.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResult(
            true,
            "Category deleted successfully.");
    }
}