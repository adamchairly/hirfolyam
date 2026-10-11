using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Application.Features;

internal sealed class GetHotTopicsQueryHandler(
    IApplicationDbContext context,
    IOptions<HotTopicsOptions> options,
    TimeProvider timeProvider) : IRequestHandler<GetHotTopicsQuery, GetHotTopicsResponse>
{
    public async Task<GetHotTopicsResponse> Handle(GetHotTopicsQuery request, CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().AddHours(-options.Value.MaxAgeHours);
        var snapshot = await context.HotTopicSnapshots
            .AsNoTracking()
            .Include(snapshot => snapshot.Topics)
            .Where(snapshot => snapshot.GeneratedAt >= cutoff)
            .OrderByDescending(snapshot => snapshot.GeneratedAt)
            .ThenByDescending(snapshot => snapshot.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (snapshot is null)
        {
            return new GetHotTopicsResponse { Items = [] };
        }

        var topics = snapshot.Topics.OrderBy(topic => topic.Rank).ToList();
        var ids = topics.SelectMany(topic => topic.ArticleIds).Distinct().ToArray();
        var articles = await context.Articles
            .AsNoTracking()
            .Where(article => ids.Contains(article.Id))
            .Select(article => new HotTopicArticleResponse
            {
                Id = article.Id,
                Title = article.Title,
                Url = article.Url,
                PublishedAt = article.PublishedAt,
                Source = new ArticleSourceResponse
                {
                    Key = article.NewsSource.Key,
                    Name = article.NewsSource.Name,
                    IconUrl = article.NewsSource.IconUrl
                }
            })
            .ToDictionaryAsync(article => article.Id, cancellationToken);

        return new GetHotTopicsResponse
        {
            GeneratedAt = snapshot.GeneratedAt,
            Items = topics
                .Select(topic => new HotTopicResponse
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    Summary = topic.Summary,
                    Articles = topic.ArticleIds
                        .Where(articles.ContainsKey)
                        .Select(id => articles[id])
                        .OrderByDescending(article => article.PublishedAt)
                        .ThenByDescending(article => article.Id)
                        .ToList()
                })
                .ToList()
        };
    }
}
