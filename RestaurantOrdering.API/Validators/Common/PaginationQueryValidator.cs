using FluentValidation;
using RestaurantOrdering.API.DTOs.Common;

namespace RestaurantOrdering.API.Validators.Common;

public class PaginationQueryValidator<T> : AbstractValidator<T>
    where T : PaginationQueryDto
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
