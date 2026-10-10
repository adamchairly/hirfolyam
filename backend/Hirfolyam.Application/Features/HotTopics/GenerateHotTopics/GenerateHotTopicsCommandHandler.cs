using System.Text.Json;
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
    ILogger<GenerateHotTopicsCommandHandler> logger) : IRequestHandler<GenerateHotTopicsCommand, GenerateHotTopicsResponse>
{
    // A story only one outlet writes about is not a hot topic.
    private const int MinSources = 2;

    public async Task<GenerateHotTopicsResponse> Handle(GenerateHotTopicsCommand request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var result = new GenerateHotTopicsResponse();
        var startedAt = timeProvider.GetTimestamp();

        var articles = await context.Articles
            .AsNoTracking()
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .Take(settings.ArticleCount)
            .Select(article => new
            {
                article.Id,
                article.NewsSourceId,
                Source = article.NewsSource.Name,
                article.Title,
                article.LeadSearchText
            })
            .ToListAsync(cancellationToken);

        if (articles.Count == 0)
        {
            return result;
        }

        HotTopicGeneration generation;

        try
        {
            generation = await generator.GenerateAsync(
                articles
                    .Select(article => new HotTopicArticle(article.Id, article.Source, article.Title, article.LeadSearchText ?? ""))
                    .ToList(),
                settings.TopicCount,
                cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // The previous snapshot stays on the page until it expires.
            logger.LogWarning(
                "Hot topic generation failed with {ErrorCode} ({ErrorType}) from {Model} after {ElapsedMilliseconds:F0} ms",
                exception switch
                {
                    HttpRequestException { StatusCode: { } status } => $"http_{(int)status}",
                    OperationCanceledException => "timeout",
                    JsonException or InvalidDataException => "invalid_response",
                    _ => "request_failed"
                },
                exception.GetType().Name,
                generator.Model,
                timeProvider.GetElapsedTime(startedAt).TotalMilliseconds);

            return result;
        }

        var sourceIds = articles.ToDictionary(article => article.Id, article => article.NewsSourceId);
        var topics = SelectTopics(generation.Topics, sourceIds, settings.TopicCount);

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
                .Select((topic, index) => new HotTopic
                {
                    Rank = index + 1,
                    Title = topic.Title,
                    Summary = topic.Summary,
                    ArticleIds = topic.ArticleIds.ToArray()
                })
                .ToList()
        });
        await context.SaveChangesAsync(cancellationToken);

        result.Succeeded = true;
        result.Topics = topics.Count;

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

        return result;
    }

    // The model groups and summarizes; the checks and the order are decided here.
    private static List<HotTopicDraft> SelectTopics(
        IReadOnlyList<HotTopicDraft> drafts,
        Dictionary<long, int> sourceIds,
        int topicCount)
    {
        var usedIds = new HashSet<long>();
        var candidates = new List<(HotTopicDraft Topic, int Sources, int Order)>();

        foreach (var draft in drafts)
        {
            // Unknown ids are dropped, and an article counts only for the first kept topic that claims it.
            var ids = draft.ArticleIds
                .Where(id => sourceIds.ContainsKey(id) && !usedIds.Contains(id))
                .Distinct()
                .ToList();
            var title = draft.Title.Trim();
            var summary = draft.Summary.Trim();
            var sources = ids.Select(id => sourceIds[id]).Distinct().Count();

            if (sources < MinSources
                || title.Length is 0 or > HotTopic.TitleMaxLength
                || summary.Length is 0 or > HotTopic.SummaryMaxLength)
            {
                continue;
            }

            usedIds.UnionWith(ids);
            candidates.Add((new HotTopicDraft(title, summary, ids), sources, candidates.Count));
        }

        return candidates
            .OrderByDescending(candidate => candidate.Sources)
            .ThenByDescending(candidate => candidate.Topic.ArticleIds.Count)
            .ThenBy(candidate => candidate.Order)
            .Take(topicCount)
            .Select(candidate => candidate.Topic)
            .ToList();
    }
}
