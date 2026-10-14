using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Application.Features;

internal sealed class GenerateHotTopicsCommandHandler(
    IApplicationDbContext context,
    IHotTopicGenerator generator,
    IOptions<HotTopicsOptions> options,
    TimeProvider timeProvider,
    ILogger<GenerateHotTopicsCommandHandler> logger) : IRequestHandler<GenerateHotTopicsCommand>
{
    public async Task Handle(GenerateHotTopicsCommand request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var startedAt = timeProvider.GetTimestamp();

        var articles = await context.Articles
            .AsNoTracking()
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .Take(settings.ArticleCount)
            .Select(article => new HotTopicArticle(
                article.Id,
                article.NewsSourceId,
                article.NewsSource.Name,
                article.Title,
                article.LeadText ?? ""))
            .ToListAsync(cancellationToken);

        if (articles.Count == 0)
        {
            logger.LogInformation("Hot topics skipped: there are no articles");

            return;
        }

        HotTopicGeneration generation;

        try
        {
            generation = await generator.GenerateAsync(articles, settings.TopicCount, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // The previous snapshot stays on the page until it expires.
            logger.LogWarning(
                exception,
                "Hot topic generation failed from {Model} after {ElapsedMilliseconds:F0} ms",
                generator.Model,
                timeProvider.GetElapsedTime(startedAt).TotalMilliseconds);

            return;
        }

        var sourceIdsByArticleId = articles.ToDictionary(article => article.Id, article => article.SourceId);
        var topics = HotTopicSelector.Select(generation.Topics, sourceIdsByArticleId, settings.TopicCount);

        // A run without a qualifying topic is stored too, so the page does not keep showing older topics.
        context.HotTopicSnapshots.Add(new HotTopicSnapshot
        {
            GeneratedAt = timeProvider.GetUtcNow(),
            Model = generator.Model,
            ArticleCount = articles.Count,
            InputTokens = generation.InputTokens,
            OutputTokens = generation.OutputTokens,
            CostUsd = generation.CostUsd,
            Topics = topics
        });
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Hot topics received from {Model} in {ElapsedMilliseconds:F0} ms: {Topics} kept of {Proposed} from {Articles} articles, input {InputTokens}, output {OutputTokens}, cost USD {CostUsd}",
            generator.Model,
            timeProvider.GetElapsedTime(startedAt).TotalMilliseconds,
            topics.Count,
            generation.Topics.Count,
            articles.Count,
            generation.InputTokens,
            generation.OutputTokens,
            generation.CostUsd);
    }
}
