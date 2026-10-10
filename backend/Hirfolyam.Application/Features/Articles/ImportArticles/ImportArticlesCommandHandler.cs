using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Application.Features;

internal sealed class ImportArticlesCommandHandler(
    IApplicationDbContext context,
    NewsSourceCatalog catalog,
    IRssFeedReader feedReader,
    IArticleImporter articleImporter,
    TimeProvider timeProvider,
    IOptions<ImportOptions> importOptions,
    ILogger<ImportArticlesCommandHandler> logger) : IRequestHandler<ImportArticlesCommand, ImportArticlesResponse>
{
    public async Task<ImportArticlesResponse> Handle(ImportArticlesCommand request, CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();
        var storedSources = await context.NewsSources
            .ToDictionaryAsync(source => source.Key, cancellationToken);
        var sources = catalog.Sources
            .Where(source => source.Enabled)
            .OrderBy(source => source.Key, StringComparer.Ordinal);
        var now = timeProvider.GetUtcNow();
        var start = now.AddHours(-importOptions.Value.LookbackHours);
        var result = new ImportArticlesResponse();

        logger.LogInformation(
            "Import started for articles published since {Start:yyyy-MM-dd HH:mm:ss}",
            start.ToLocalTime());

        foreach (var source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var sourceScope = logger.BeginScope(new Dictionary<string, object>
            {
                ["NewsSource"] = source.Key
            });

            if (!storedSources.TryGetValue(source.Key, out var storedSource))
            {
                result.FailedFeeds += source.Feeds.Count;
                logger.LogError("News source {NewsSourceKey} is not stored; synchronize the sources first", source.Key);

                continue;
            }

            var failedFeedsBefore = result.FailedFeeds;

            foreach (var feedUrl in source.Feeds)
            {
                IReadOnlyCollection<RssArticle> articles;

                try
                {
                    articles = await feedReader.ReadAsync(feedUrl, cancellationToken);
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    result.FailedFeeds++;
                    logger.LogError(exception, "Feed import failed for {FeedUrl}", feedUrl);

                    continue;
                }

                foreach (var article in articles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!article.PublishedAt.HasValue)
                    {
                        result.Skipped++;
                        logger.LogWarning("Article has no publication date: {ArticleUrl}", article.Url);

                        continue;
                    }

                    if (article.PublishedAt < start || article.PublishedAt > now)
                    {
                        result.Skipped++;
                        continue;
                    }

                    try
                    {
                        var imported = await articleImporter.ImportAsync(
                            storedSource.Id,
                            source.Key,
                            article,
                            cancellationToken);

                        if (imported)
                        {
                            result.Imported++;
                        }
                        else
                        {
                            result.Duplicates++;
                        }
                    }
                    catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                    {
                        result.FailedArticles++;
                        logger.LogError(exception, "Article import failed for {ArticleUrl}", article.Url);
                    }
                }
            }

            if (result.FailedFeeds == failedFeedsBefore)
            {
                storedSource.LastImportedAt = timeProvider.GetUtcNow();
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        var elapsed = timeProvider.GetElapsedTime(startedAt);
        var hasFailures = result.FailedFeeds > 0 || result.FailedArticles > 0;
        var completionLogLevel = hasFailures ? LogLevel.Warning : LogLevel.Information;

        logger.Log(
            completionLogLevel,
            "Import finished in {ElapsedMilliseconds:F0} ms: {Imported} imported, {Duplicates} duplicates, {Skipped} skipped, {FailedFeeds} failed feeds, {FailedArticles} failed articles",
            elapsed.TotalMilliseconds,
            result.Imported,
            result.Duplicates,
            result.Skipped,
            result.FailedFeeds,
            result.FailedArticles);

        return result;
    }
}
