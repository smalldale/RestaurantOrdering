using FluentValidation;
using RestaurantOrdering.API.DTOs.Order;

namespace RestaurantOrdering.API.Validators.Order;

public class UpdateOrderStatusValidator
    : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(
                "Status must be one of: Pending, Preparing, Ready, Completed, Canceled.");
    }
}
