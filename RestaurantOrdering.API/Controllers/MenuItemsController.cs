using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IValidator<MenuItemQueryDto> _queryValidator;
    private readonly IValidator<CreateMenuItemDto> _createValidator;
    private readonly IValidator<UpdateMenuItemDto> _updateValidator;

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

    // GET: api/menuitems
    // Admin and Staff
    [HttpGet]
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

    // GET: api/menuitems/{id}
    // Admin and Staff
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _menuService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    // POST: api/menuitems
    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
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
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data?.Id },
            result);
    }

    // PUT: api/menuitems/{id}
    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
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
            return BadRequest(result);

        return Ok(result);
    }

    // DELETE: api/menuitems/{id}
    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _menuService.DeleteAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}