using FluentValidation;
using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.Validators.Common;

namespace RestaurantOrdering.API.Validators.Category;

public class CategoryQueryValidator
    : PaginationQueryValidator<CategoryQueryDto>
{
    public CategoryQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.");
    }
}
