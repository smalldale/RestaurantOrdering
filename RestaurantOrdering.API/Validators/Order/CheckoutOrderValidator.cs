using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class CheckoutOrderValidator
    : AbstractValidator<CheckoutOrderDto>
{
    public CheckoutOrderValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Invalid payment method.");
    }
}
