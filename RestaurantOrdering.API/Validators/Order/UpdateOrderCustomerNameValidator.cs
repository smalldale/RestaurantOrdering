using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class UpdateOrderCustomerNameValidator
    : AbstractValidator<UpdateOrderCustomerNameDto>
{
    public UpdateOrderCustomerNameValidator()
    {
        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .WithMessage("Customer name cannot exceed 100 characters.");
    }
}
