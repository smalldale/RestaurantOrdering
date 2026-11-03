using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

/// <summary>
/// Manages menu items.
/// Read operations are available to Admin and Staff; mutations are Admin only.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IValidator<MenuItemQueryDto> _queryValidator;
    private readonly IValidator<CreateMenuItemDto> _createValidator;
    private readonly IValidator<UpdateMenuItemDto> _updateValidator;

    /// <summary>
    /// Initializes a new instance of <see cref="MenuItemsController"/>.
    /// </summary>
    /// <param name="menuService">Service that performs menu item operations.</param>
    /// <param name="queryValidator">Validator for menu item list queries.</param>
    /// <param name="createValidator">Validator for menu item creation.</param>
    /// <param name="updateValidator">Validator for menu item updates.</param>
    public MenuItemsController(
        IMenuService menuService,
        IValidator<MenuItemQueryDto> queryValidator,
        IValidator<CreateMenuItemDto> createValidator,
        IValidator<UpdateMenuItemDto> updateValidator)
    {
        _menuService = menuService;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Returns a paged list of menu items with optional search and filters.
    /// </summary>
    /// <param name="query">Paging, search, category, and availability parameters.</param>
    /// <returns>A page of menu items.</returns>
    /// <response code="200">Menu items were retrieved.</response>
    /// <response code="400">The query parameters failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<MenuItemResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MenuItemQueryDto query)
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

        var result = await _menuService.GetAllAsync(query);

        return Ok(result);
    }

    /// <summary>
    /// Returns a single menu item by its identifier.
    /// </summary>
    /// <param name="id">Menu item identifier.</param>
    /// <returns>The requested menu item, including its category name.</returns>
    /// <response code="200">The menu item was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No menu item exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<MenuItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _menuService.GetByIdAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Creates a menu item. Admin only.
    /// </summary>
    /// <param name="dto">Menu item data.</param>
    /// <returns>The created menu item.</returns>
    /// <response code="201">The menu item was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    /// <response code="404">The referenced category does not exist.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<MenuItemResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMenuItemDto dto)
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

        var result = await _menuService.CreateAsync(dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data?.Id },
            result);
    }

    /// <summary>
    /// Updates an existing menu item. Admin only.
    /// </summary>
    /// <param name="id">Menu item identifier.</param>
    /// <param name="dto">New menu item data.</param>
    /// <returns>The updated menu item.</returns>
    /// <response code="200">The menu item was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    /// <response code="404">The menu item or referenced category does not exist.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<MenuItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMenuItemDto dto)
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

        var result = await _menuService.UpdateAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Deletes a menu item. Admin only.
    /// </summary>
    /// <remarks>
    /// A menu item cannot be deleted when existing order lines reference it.
    /// </remarks>
    /// <param name="id">Menu item identifier.</param>
    /// <returns>A confirmation that the menu item was deleted.</returns>
    /// <response code="200">The menu item was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin.</response>
    /// <response code="404">No menu item exists with the supplied identifier.</response>
    /// <response code="409">The menu item is referenced by existing order lines.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _menuService.DeleteAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
