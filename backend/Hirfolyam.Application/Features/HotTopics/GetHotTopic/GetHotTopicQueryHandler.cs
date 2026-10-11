using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class GetHotTopicQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetHotTopicQuery, GetHotTopicResponse>
{
    public async Task<GetHotTopicResponse> Handle(GetHotTopicQuery request, CancellationToken cancellationToken)
    {
        var entry = await context.HotTopicSnapshots
            .AsNoTracking()
            .SelectMany(snapshot => snapshot.Topics, (snapshot, topic) => new
            {
                snapshot.GeneratedAt,
                Topic = topic
            })
            .FirstOrDefaultAsync(entry => entry.Topic.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Hot topic {request.Id} was not found.");

        var ids = entry.Topic.ArticleIds;
        var articles = await context.Articles
            .AsNoTracking()
            .Where(article => ids.Contains(article.Id))
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
            .ToListAsync(cancellationToken);

        return new GetHotTopicResponse
        {
            Id = entry.Topic.Id,
            Title = entry.Topic.Title,
            Summary = entry.Topic.Summary,
            GeneratedAt = entry.GeneratedAt,
            Articles = articles
        };
    }
}
