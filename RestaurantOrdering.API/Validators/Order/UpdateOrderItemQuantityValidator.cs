using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class UpdateOrderItemQuantityValidator
    : AbstractValidator<UpdateOrderItemQuantityDto>
{
    public UpdateOrderItemQuantityValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
