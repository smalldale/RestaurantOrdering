using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrdering.API.DTOs.Auth;
using RestaurantOrdering.API.DTOs.Common;
using RestaurantOrdering.API.Services.Interfaces;

namespace RestaurantOrdering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<RefreshTokenDto> _refreshTokenValidator;

    public AuthController(
        IAuthService authService,
        IValidator<LoginDto> loginValidator,
        IValidator<RefreshTokenDto> refreshTokenValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
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

    [AllowAnonymous]
    [HttpPost("refresh")]
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

    [AllowAnonymous]
    [HttpPost("logout")]
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