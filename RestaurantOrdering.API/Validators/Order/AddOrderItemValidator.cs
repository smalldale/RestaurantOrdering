using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class AddOrderItemValidator
    : AbstractValidator<AddOrderItemDto>
{
    public AddOrderItemValidator()
    {
        RuleFor(x => x.MenuItemId)
            .GreaterThan(0)
            .WithMessage("Menu item ID must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
