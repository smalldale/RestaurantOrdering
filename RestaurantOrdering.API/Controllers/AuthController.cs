using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Auth;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

/// <summary>
/// Authentication endpoints for internal users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<RefreshTokenDto> _refreshTokenValidator;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Service that performs authentication operations.</param>
    /// <param name="loginValidator">Validator for login requests.</param>
    /// <param name="refreshTokenValidator">Validator for refresh-token requests.</param>
    public AuthController(
        IAuthService authService,
        IValidator<LoginDto> loginValidator,
        IValidator<RefreshTokenDto> refreshTokenValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    /// <summary>
    /// Authenticates a user and returns an access token and refresh token.
    /// </summary>
    /// <param name="dto">Email and password credentials.</param>
    /// <returns>The issued tokens and the authenticated user's identity.</returns>
    /// <response code="200">Login succeeded.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The email or password is invalid, or the account is inactive.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ValidationError(validationResult);
        }

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access token and refresh token.
    /// </summary>
    /// <param name="dto">The refresh token to rotate.</param>
    /// <returns>A new access token and refresh token.</returns>
    /// <response code="200">The token was refreshed.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The refresh token is invalid, expired, or already revoked.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ValidationError(validationResult);
        }

        var result = await _authService.RefreshAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Revokes a refresh token. The operation is idempotent.
    /// </summary>
    /// <param name="dto">The refresh token to revoke.</param>
    /// <returns>A confirmation that logout succeeded.</returns>
    /// <response code="200">Logout succeeded, even if the token was already revoked or unknown.</response>
    /// <response code="400">The request failed validation.</response>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ValidationError(validationResult);
        }

        var result = await _authService.LogoutAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private static BadRequestObjectResult ValidationError(
        FluentValidation.Results.ValidationResult validation)
    {
        var errors = validation.Errors
            .Select(x => x.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(
            ApiResponseDto<object>.ErrorResult(
                "Validation failed.",
                errors));
    }
}
