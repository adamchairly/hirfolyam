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
        var topics = SelectTopics(generation.Topics, sourceIdsByArticleId, settings.TopicCount);

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

    // The model groups and summarizes; the checks and the order are decided here.
    private static List<HotTopic> SelectTopics(
        IReadOnlyList<HotTopicDraft> drafts,
        Dictionary<long, int> sourceIdsByArticleId,
        int topicCount)
    {
        var usedIds = new HashSet<long>();
        var candidates = new List<(string Title, string Summary, List<long> ArticleIds, int Sources)>();

        foreach (var draft in drafts)
        {
            // Unknown ids are dropped, and an article counts only for the first kept topic that claims it.
            var ids = draft.ArticleIds
                .Where(id => sourceIdsByArticleId.ContainsKey(id) && !usedIds.Contains(id))
                .Distinct()
                .ToList();
            var title = draft.Title.Trim();
            var summary = draft.Summary.Trim();
            var sources = ids.Select(id => sourceIdsByArticleId[id]).Distinct().Count();

            if (sources < HotTopic.MinSources
                || title.Length is 0 or > HotTopic.TitleMaxLength
                || summary.Length is 0 or > HotTopic.SummaryMaxLength)
            {
                continue;
            }

            usedIds.UnionWith(ids);
            candidates.Add((title, summary, ids, sources));
        }

        // The sort is stable, so equal topics keep the order of the model.
        return candidates
            .OrderByDescending(candidate => candidate.Sources)
            .ThenByDescending(candidate => candidate.ArticleIds.Count)
            .Take(topicCount)
            .Select((candidate, index) => new HotTopic
            {
                Rank = index + 1,
                Title = candidate.Title,
                Summary = candidate.Summary,
                ArticleIds = [.. candidate.ArticleIds]
            })
            .ToList();
    }
}
