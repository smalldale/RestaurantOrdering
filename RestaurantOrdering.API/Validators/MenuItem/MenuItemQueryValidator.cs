using FluentValidation;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.Validators.Common;

namespace RestaurantOrdering.API.Validators.MenuItem;

public class MenuItemQueryValidator
    : PaginationQueryValidator<MenuItemQueryDto>
{
    public MenuItemQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("Category ID must be greater than 0.");
    }
}
