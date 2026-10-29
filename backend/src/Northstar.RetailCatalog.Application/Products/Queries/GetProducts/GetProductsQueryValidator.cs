using FluentValidation;

namespace Northstar.RetailCatalog.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(q => q.Request.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page должен быть не меньше 1.");

        RuleFor(q => q.Request.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("pageSize должен быть не меньше 1.")
            .LessThanOrEqualTo(Application.Common.ProductQueryRequest.MaxPageSize)
            .WithMessage($"pageSize не может превышать {Application.Common.ProductQueryRequest.MaxPageSize}.");

        RuleFor(q => q.Request.SortBy)
            .Must(sortBy => Application.Common.ProductSortFields.Allowed.Contains(sortBy))
            .WithMessage($"sortBy допускается только из списка: {string.Join(", ", Application.Common.ProductSortFields.Allowed)}.")
            .When(q => !string.IsNullOrEmpty(q.Request.SortBy));

        RuleFor(q => q.Request.SortDirection)
            .Must(direction => direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("sortDirection допускается только asc или desc.");
    }
}
