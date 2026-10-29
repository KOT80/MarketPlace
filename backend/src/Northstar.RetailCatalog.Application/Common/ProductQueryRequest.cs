namespace Northstar.RetailCatalog.Application.Common;

public sealed record ProductQueryRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public bool? IsActive { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;

    public string SortBy { get; init; } = ProductSortFields.CreatedAt;

    public string SortDirection { get; init; } = "desc";
}
