using FluentValidation;
using RestaurantOrdering.API.DTOs.Auth;

namespace RestaurantOrdering.API.Validators.Auth;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}