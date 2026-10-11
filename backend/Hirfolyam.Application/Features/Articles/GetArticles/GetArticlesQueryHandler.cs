using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Extensions;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class GetArticlesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetArticlesQuery, PagedResult<ArticleListItemResponse>>
{
    public async Task<PagedResult<ArticleListItemResponse>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var articles = context.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = NormalizeKey(request.Category);

            articles = articles.Where(article =>
                article.Category != null && article.Category.Slug == category);
        }

        if (request.ExcludedCategories is not null)
        {
            var excludedCategories = NormalizeKeys(request.ExcludedCategories);

            if (excludedCategories.Length > 0)
            {
                articles = articles.Where(article =>
                    article.Category == null || !excludedCategories.Contains(article.Category.Slug));
            }
        }

        if (request.Sources is not null)
        {
            var sources = NormalizeKeys(request.Sources);

            if (sources.Length == 0)
            {
                return PagedResult<ArticleListItemResponse>.Empty(request.Page, request.PageSize);
            }

            articles = articles.Where(article => sources.Contains(article.NewsSource.Key));
        }

        if (request.Ids is not null)
        {
            var ids = request.Ids.Distinct().ToArray();

            articles = articles.Where(article => ids.Contains(article.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = ContainsPattern(request.Search);

            articles = articles.Where(article =>
                EF.Functions.ILike(article.Title, pattern, "\\")
                || (article.LeadText != null
                    && EF.Functions.ILike(article.LeadText, pattern, "\\")));
        }

        return await articles
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .Select(article => new ArticleListItemResponse
            {
                Id = article.Id,
                Title = article.Title,
                Lead = article.Lead,
                Source = new ArticleSourceResponse
                {
                    Key = article.NewsSource.Key,
                    Name = article.NewsSource.Name,
                    IconUrl = article.NewsSource.IconUrl
                },
                Category = article.Category == null
                    ? null
                    : new ArticleCategoryResponse
                    {
                        Slug = article.Category.Slug,
                        Name = article.Category.Name
                    },
                Author = article.Author,
                ImageUrl = article.ImageUrl,
                PublishedAt = article.PublishedAt,
                Url = article.Url
            })
            .ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);
    }

    private static string NormalizeKey(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static string[] NormalizeKeys(string?[] values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeKey(value!))
            .Distinct()
            .ToArray();
    }

    // Matches the search text anywhere; the LIKE wildcards in it are escaped with a backslash.
    private static string ContainsPattern(string search)
    {
        var escaped = search.Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }
}
