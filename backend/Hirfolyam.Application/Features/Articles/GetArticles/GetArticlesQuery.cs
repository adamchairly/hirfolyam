using Hirfolyam.Application.Shared;
using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GetArticlesQuery : IRequest<PagedResult<ArticleListItemResponse>>
{
    public string? Category { get; init; }

    // Articles in these categories are left out. Articles without a category are kept.
    public string?[]? ExcludedCategories { get; init; }

    // A missing list includes all sources. An empty list includes none.
    public string?[]? Sources { get; init; }

    // A missing list includes all articles.
    public long[]? Ids { get; init; }

    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

public sealed class ArticleListItemResponse
{
    public long Id { get; init; }

    public required string Title { get; init; }

    public string? Lead { get; init; }

    public required ArticleSourceResponse Source { get; init; }

    public ArticleCategoryResponse? Category { get; init; }

    public string? Author { get; init; }

    public string? ImageUrl { get; init; }

    public DateTimeOffset PublishedAt { get; init; }

    public required string Url { get; init; }
}

public sealed class ArticleSourceResponse
{
    public required string Key { get; init; }

    public required string Name { get; init; }

    public string? IconUrl { get; init; }
}

public sealed class ArticleCategoryResponse
{
    public required string Slug { get; init; }

    public required string Name { get; init; }
}
