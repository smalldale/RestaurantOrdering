using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class CreateOrderValidator
    : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .WithMessage("Customer name cannot exceed 100 characters.");
    }
}
