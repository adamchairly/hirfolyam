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
    ArticleImporter articleImporter,
    TimeProvider timeProvider,
    IOptions<ImportOptions> options,
    ILogger<ImportArticlesCommandHandler> logger) : IRequestHandler<ImportArticlesCommand, ImportArticlesResult>
{
    public async Task<ImportArticlesResult> Handle(ImportArticlesCommand request, CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();
        var storedSources = await context.NewsSources
            .ToDictionaryAsync(source => source.Key, cancellationToken);
        var sources = catalog.Sources
            .Where(source => source.IsEnabled)
            .OrderBy(source => source.Key, StringComparer.Ordinal);
        var end = timeProvider.GetUtcNow();
        var start = end.AddHours(-options.Value.LookbackHours);
        var result = new ImportArticlesResult();

        logger.LogInformation(
            "Import started for articles published since {Start:yyyy-MM-dd HH:mm:ss}",
            start.ToLocalTime());

        foreach (var source in sources)
        {
            using var sourceScope = logger.BeginScope(new Dictionary<string, object>
            {
                ["NewsSource"] = source.Key
            });

            // The startup synchronization stores every source of the catalog.
            var storedSource = storedSources[source.Key];
            var allFeedsRead = true;

            foreach (var feedUrl in source.Feeds)
            {
                allFeedsRead &= await ImportFeedAsync(storedSource, feedUrl, start, end, result, cancellationToken);
            }

            if (allFeedsRead)
            {
                storedSource.LastImportedAt = timeProvider.GetUtcNow();
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        var hasFailures = result.FailedFeeds > 0 || result.FailedArticles > 0;

        logger.Log(
            hasFailures ? LogLevel.Warning : LogLevel.Information,
            "Import finished in {ElapsedMilliseconds:F0} ms: {Imported} imported, {Duplicates} duplicates, {Skipped} skipped, {FailedFeeds} failed feeds, {FailedArticles} failed articles",
            timeProvider.GetElapsedTime(startedAt).TotalMilliseconds,
            result.Imported,
            result.Duplicates,
            result.Skipped,
            result.FailedFeeds,
            result.FailedArticles);

        return result;
    }

    // Returns false when the feed could not be read.
    private async Task<bool> ImportFeedAsync(
        NewsSource source,
        string feedUrl,
        DateTimeOffset start,
        DateTimeOffset end,
        ImportArticlesResult result,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RssArticle> articles;

        try
        {
            articles = await feedReader.ReadAsync(feedUrl, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            result.FailedFeeds++;
            logger.LogWarning(exception, "Feed import failed for {FeedUrl}", feedUrl);

            return false;
        }

        foreach (var article in articles)
        {
            if (article.PublishedAt < start || article.PublishedAt > end)
            {
                result.Skipped++;

                continue;
            }

            await ImportArticleAsync(source, article, result, cancellationToken);
        }

        return true;
    }

    private async Task ImportArticleAsync(
        NewsSource source,
        RssArticle article,
        ImportArticlesResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await articleImporter.ImportAsync(source, article, cancellationToken))
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
