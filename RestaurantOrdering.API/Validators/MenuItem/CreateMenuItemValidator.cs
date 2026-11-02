using FluentValidation;
using RestaurantOrdering.API.DTOs.MenuItem;

namespace RestaurantOrdering.API.Validators.MenuItem;

public class CreateMenuItemValidator
    : AbstractValidator<CreateMenuItemDto>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Menu item name is required.")
            .MaximumLength(100)
            .WithMessage("Menu item name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category ID must be greater than 0.");
    }
}
