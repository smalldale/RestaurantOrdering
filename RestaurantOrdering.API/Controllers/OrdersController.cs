using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.Order;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<AddOrderItemDto> _addItemValidator;
    private readonly IValidator<CheckoutOrderDto> _checkoutValidator;
    private readonly IValidator<UpdateOrderCustomerNameDto> _updateCustomerNameValidator;
    private readonly IValidator<UpdateOrderStatusDto> _updateStatusValidator;
    private readonly IValidator<UpdateOrderItemQuantityDto> _updateItemQuantityValidator;

    public OrdersController(
        IOrderService orderService,
        IValidator<CreateOrderDto> createValidator,
        IValidator<AddOrderItemDto> addItemValidator,
        IValidator<CheckoutOrderDto> checkoutValidator,
        IValidator<UpdateOrderCustomerNameDto> updateCustomerNameValidator,
        IValidator<UpdateOrderStatusDto> updateStatusValidator,
        IValidator<UpdateOrderItemQuantityDto> updateItemQuantityValidator)
    {
        _orderService = orderService;
        _createValidator = createValidator;
        _addItemValidator = addItemValidator;
        _checkoutValidator = checkoutValidator;
        _updateCustomerNameValidator = updateCustomerNameValidator;
        _updateStatusValidator = updateStatusValidator;
        _updateItemQuantityValidator = updateItemQuantityValidator;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();

        return Ok(result);
    }

    // GET: api/orders/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.CreateAsync(dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data?.Id },
            result);
    }

    // PUT: api/orders/{id}/customer
    [HttpPut("{id:int}/customer")]
    public async Task<IActionResult> UpdateCustomerName(
        int id,
        [FromBody] UpdateOrderCustomerNameDto dto)
    {
        var validation = await _updateCustomerNameValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.UpdateCustomerNameAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // PUT: api/orders/{id}/status
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        var validation = await _updateStatusValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.UpdateStatusAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // PUT: api/orders/{id}/items/{orderItemId}
    [HttpPut("{id:int}/items/{orderItemId:int}")]
    public async Task<IActionResult> UpdateItemQuantity(
        int id,
        int orderItemId,
        [FromBody] UpdateOrderItemQuantityDto dto)
    {
        var validation = await _updateItemQuantityValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.UpdateItemQuantityAsync(
            id,
            orderItemId,
            dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // POST: api/orders/{id}/items
    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AddItem(
        int id,
        [FromBody] AddOrderItemDto dto)
    {
        var validation = await _addItemValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.AddItemAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // DELETE: api/orders/{id}/items/{orderItemId}
    [HttpDelete("{id:int}/items/{orderItemId:int}")]
    public async Task<IActionResult> RemoveItem(int id, int orderItemId)
    {
        var result = await _orderService.RemoveItemAsync(id, orderItemId);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // POST: api/orders/{id}/checkout
    [HttpPost("{id:int}/checkout")]
    public async Task<IActionResult> Checkout(
        int id,
        [FromBody] CheckoutOrderDto dto)
    {
        var validation = await _checkoutValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationError(validation);

        var result = await _orderService.CheckoutAsync(id, dto);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // DELETE: api/orders/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.DeleteAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    private BadRequestObjectResult ValidationError(
        FluentValidation.Results.ValidationResult validation)
    {
        var errors = validation.Errors
            .Select(x => x.ErrorMessage)
            .ToList();

        return BadRequest(ApiResponseDto<object>.ErrorResult(
            "Validation failed.",
            errors));
    }
}
