using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.DTOs.Order;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

/// <summary>
/// Creates and processes customer orders.
/// All endpoints require an authenticated Admin or Staff user.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of <see cref="OrdersController"/>.
    /// </summary>
    /// <param name="orderService">Service that performs order operations.</param>
    /// <param name="createValidator">Validator for order creation.</param>
    /// <param name="addItemValidator">Validator for adding an order item.</param>
    /// <param name="checkoutValidator">Validator for checkout.</param>
    /// <param name="updateCustomerNameValidator">Validator for customer name updates.</param>
    /// <param name="updateStatusValidator">Validator for status updates.</param>
    /// <param name="updateItemQuantityValidator">Validator for order item quantity updates.</param>
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

    /// <summary>
    /// Returns all orders, newest first, including their order lines.
    /// </summary>
    /// <returns>All orders.</returns>
    /// <response code="200">Orders were retrieved.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<OrderResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();

        return Ok(result);
    }

    /// <summary>
    /// Returns a single order by its identifier.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>The requested order with its order lines.</returns>
    /// <response code="200">The order was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new pending order.
    /// </summary>
    /// <param name="dto">Optional customer name.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Updates the customer name of a pending order.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="dto">New customer name. Empty or whitespace clears it.</param>
    /// <returns>The updated order.</returns>
    /// <response code="200">The customer name was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">The order is no longer pending.</response>
    [HttpPut("{id:int}/customer")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Moves an order to a new lifecycle status.
    /// </summary>
    /// <remarks>
    /// Allowed transitions:
    /// <c>Pending -> Preparing</c>, <c>Pending -> Canceled</c>,
    /// <c>Preparing -> Ready</c>, <c>Preparing -> Canceled</c>,
    /// <c>Ready -> Completed</c>, and <c>Ready -> Canceled</c>.
    /// <c>Completed</c> and <c>Canceled</c> are terminal.
    /// Moving from <c>Pending</c> to <c>Preparing</c> requires at least one order item.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="dto">Target status.</param>
    /// <returns>The updated order.</returns>
    /// <response code="200">The status was updated.</response>
    /// <response code="400">The request failed validation, or the order has no items.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">The status is unchanged, the order is terminal, or the transition is invalid.</response>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Changes the quantity of an existing order line.
    /// Only pending orders can be changed.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="orderItemId">Order line identifier.</param>
    /// <param name="dto">New quantity, which must be greater than zero.</param>
    /// <returns>The updated order with recalculated totals.</returns>
    /// <response code="200">The quantity was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">The order or order line does not exist.</response>
    /// <response code="409">The order is no longer pending.</response>
    [HttpPut("{id:int}/items/{orderItemId:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Adds a menu item to a pending order.
    /// If the item is already on the order, its quantity is increased.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="dto">Menu item and quantity to add.</param>
    /// <returns>The updated order with recalculated totals.</returns>
    /// <response code="200">The item was added.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">The order or menu item does not exist.</response>
    /// <response code="409">The order is not pending or the menu item is unavailable.</response>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Removes an item from a pending order.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="orderItemId">Order line identifier.</param>
    /// <returns>The updated order with recalculated totals.</returns>
    /// <response code="200">The item was removed.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">The order or order line does not exist.</response>
    /// <response code="409">The order is no longer pending.</response>
    [HttpDelete("{id:int}/items/{orderItemId:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveItem(int id, int orderItemId)
    {
        var result = await _orderService.RemoveItemAsync(id, orderItemId);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Checks out a pending order, records payment, and moves it to <c>Preparing</c>.
    /// The order must contain at least one item.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="dto">Payment method.</param>
    /// <returns>The checked-out order.</returns>
    /// <response code="200">The order was checked out.</response>
    /// <response code="400">The request failed validation or the order has no items.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">The order is no longer pending.</response>
    [HttpPost("{id:int}/checkout")]
    [ProducesResponseType(typeof(ApiResponseDto<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Deletes a pending order and its order lines.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>A confirmation that the order was deleted.</returns>
    /// <response code="200">The order was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is authenticated but is not an Admin or Staff user.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">The order is no longer pending.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
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
