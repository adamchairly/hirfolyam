using Hirfolyam.Application.Abstractions;
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
            var category = request.Category.Trim().ToLowerInvariant();

            articles = articles.Where(article =>
                article.Category != null && article.Category.Slug == category);
        }

        if (request.ExcludedCategories is not null)
        {
            var excludedCategories = request.ExcludedCategories
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Select(category => category!.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            if (excludedCategories.Length > 0)
            {
                articles = articles.Where(article =>
                    article.Category == null || !excludedCategories.Contains(article.Category.Slug));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim().ToLowerInvariant();

            articles = articles.Where(article => article.NewsSource.Key == source);
        }

        if (request.Sources is not null)
        {
            var sources = request.Sources
                .Where(source => !string.IsNullOrWhiteSpace(source))
                .Select(source => source!.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            articles = sources.Length == 0
                ? articles.Where(article => false)
                : articles.Where(article => sources.Contains(article.NewsSource.Key));
        }

        if (request.Ids is not null)
        {
            var ids = request.Ids.Distinct().ToArray();

            articles = articles.Where(article => ids.Contains(article.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim()
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
            var pattern = $"%{search}%";

            articles = articles.Where(article =>
                EF.Functions.ILike(article.Title, pattern, "\\")
                || (article.LeadSearchText != null
                    && EF.Functions.ILike(article.LeadSearchText, pattern, "\\")));
        }

        var totalItems = await articles.CountAsync(cancellationToken);
        var offset = checked((request.Page - 1) * request.PageSize);
        var items = await articles
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .Skip(offset)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleListItemResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems
        };
    }
}
