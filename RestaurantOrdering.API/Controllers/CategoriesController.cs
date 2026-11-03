using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

/// <summary>
/// Manages menu categories.
/// Read operations are available to Admin and Staff; mutations are Admin only.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CategoryQueryDto> _queryValidator;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    /// <summary>
    /// Initializes a new instance of <see cref="CategoriesController"/>.
    /// </summary>
    /// <param name="categoryService">Service that performs category operations.</param>
    /// <param name="queryValidator">Validator for category list queries.</param>
    /// <param name="createValidator">Validator for category creation.</param>
    /// <param name="updateValidator">Validator for category updates.</param>
    public CategoriesController(
        ICategoryService categoryService,
        IValidator<CategoryQueryDto> queryValidator,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator)
    {
        _categoryService = categoryService;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Returns a paged list of categories, optionally filtered by name.
    /// </summary>
    /// <param name="query">Paging and optional search parameters.</param>
    /// <returns>A page of categories.</returns>
    /// <response code="200">Categories were retrieved.</response>
    /// <response code="400">The query parameters failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CategoryQueryDto query)
    {
        var validation = await _queryValidator.ValidateAsync(query);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(x => x.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<object>.ErrorResult(
                "Validation failed.",
                errors));
        }

        var result = await _categoryService.GetAllAsync(query);

        return Ok(result);
    }

    /// <summary>
    /// Returns a single category by its identifier.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <returns>The requested category.</returns>
    /// <response code="200">The category was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No category exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Creates a category. Admin only.
    /// </summary>
    /// <param name="dto">Category data.</param>
    /// <returns>The created category.</returns>
    /// <response code="201">The category was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(x => x.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<object>.ErrorResult(
                "Validation failed.",
                errors));
        }

        var result = await _categoryService.CreateAsync(dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data?.Id },
            result);
    }

    /// <summary>
    /// Updates an existing category. Admin only.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <param name="dto">New category data.</param>
    /// <returns>The updated category.</returns>
    /// <response code="200">The category was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    /// <response code="404">No category exists with the supplied identifier.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(x => x.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<object>.ErrorResult(
                "Validation failed.",
                errors));
        }

        var result = await _categoryService.UpdateAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Deletes a category. Admin only.
    /// </summary>
    /// <remarks>
    /// A category cannot be deleted while it still contains menu items.
    /// </remarks>
    /// <param name="id">Category identifier.</param>
    /// <returns>A confirmation that the category was deleted.</returns>
    /// <response code="200">The category was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    /// <response code="404">No category exists with the supplied identifier.</response>
    /// <response code="409">The category still contains menu items.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
