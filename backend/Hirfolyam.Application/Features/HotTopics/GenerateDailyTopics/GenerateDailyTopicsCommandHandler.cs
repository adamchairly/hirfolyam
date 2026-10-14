using Hirfolyam.Application.Abstractions;
using Hirfolyam.Application.Options;
using Hirfolyam.Application.Shared;
using Hirfolyam.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hirfolyam.Application.Features;

// Summarizes the hourly topics of the day so far. Every run adds a snapshot; the page shows the newest of the day.
internal sealed class GenerateDailyTopicsCommandHandler(
    IApplicationDbContext context,
    IHotTopicGenerator generator,
    IOptions<HotTopicsOptions> options,
    TimeProvider timeProvider,
    ILogger<GenerateDailyTopicsCommandHandler> logger) : IRequestHandler<GenerateDailyTopicsCommand>
{
    public async Task Handle(GenerateDailyTopicsCommand request, CancellationToken cancellationToken)
    {
        var today = BudapestTime.GetDay(timeProvider.GetUtcNow());
        var yesterday = today.AddDays(-1);

        // A day without any summary, like the one before the first run, is summarized late rather than never.
        if (!await context.HotTopicSnapshots.AnyAsync(snapshot => snapshot.Day == yesterday, cancellationToken))
        {
            await SummarizeAsync(yesterday, cancellationToken);
        }

        await SummarizeAsync(today, cancellationToken);
    }

    private async Task SummarizeAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var startedAt = timeProvider.GetTimestamp();
        var start = BudapestTime.GetStart(day);
        var end = BudapestTime.GetStart(day.AddDays(1));

        var hourlyTopics = await context.HotTopicSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.Day == null && snapshot.GeneratedAt >= start && snapshot.GeneratedAt < end)
            .SelectMany(snapshot => snapshot.Topics, (snapshot, topic) => new
            {
                snapshot.GeneratedAt,
                Topic = topic
            })
            .OrderBy(entry => entry.GeneratedAt)
            .ThenBy(entry => entry.Topic.Rank)
            .Select(entry => entry.Topic)
            .ToListAsync(cancellationToken);

        if (hourlyTopics.Count == 0)
        {
            logger.LogInformation("Daily topics of {Day} skipped: there are no hourly topics", day);

            return;
        }

        DailyTopicGeneration generation;

        try
        {
            generation = await generator.GenerateDailyAsync(
                hourlyTopics.Select(topic => new DailyTopicInput(topic.Id, topic.Title, topic.Summary)).ToList(),
                settings.TopicCount,
                cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // The earlier summary of the day stays on the page.
            logger.LogWarning(
                exception,
                "Daily topic generation of {Day} failed from {Model} after {ElapsedMilliseconds:F0} ms",
                day,
                generator.Model,
                timeProvider.GetElapsedTime(startedAt).TotalMilliseconds);

            return;
        }

        var articleIds = hourlyTopics.SelectMany(topic => topic.ArticleIds).Distinct().ToArray();
        var sourceIdsByArticleId = await context.Articles
            .AsNoTracking()
            .Where(article => articleIds.Contains(article.Id))
            .Select(article => new { article.Id, article.NewsSourceId })
            .ToDictionaryAsync(article => article.Id, article => article.NewsSourceId, cancellationToken);

        // A story gets the articles of the hourly topics it merges; unknown topic ids are dropped.
        var articleIdsByTopicId = hourlyTopics.ToDictionary(topic => topic.Id, topic => topic.ArticleIds);
        var drafts = generation.Topics
            .Select(draft => new HotTopicDraft(
                draft.Title,
                draft.Summary,
                draft.TopicIds
                    .Where(articleIdsByTopicId.ContainsKey)
                    .SelectMany(id => articleIdsByTopicId[id])
                    .ToList()))
            .ToList();
        var topics = HotTopicSelector.Select(drafts, sourceIdsByArticleId, settings.TopicCount);

        context.HotTopicSnapshots.Add(new HotTopicSnapshot
        {
            Day = day,
            GeneratedAt = timeProvider.GetUtcNow(),
            Model = generator.Model,
            ArticleCount = sourceIdsByArticleId.Count,
            InputTokens = generation.InputTokens,
            OutputTokens = generation.OutputTokens,
            CostUsd = generation.CostUsd,
            Topics = topics
        });
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Daily topics of {Day} received from {Model} in {ElapsedMilliseconds:F0} ms: {Topics} kept of {Proposed} from {HourlyTopics} hourly topics, input {InputTokens}, output {OutputTokens}, cost USD {CostUsd}",
            day,
            generator.Model,
            timeProvider.GetElapsedTime(startedAt).TotalMilliseconds,
            topics.Count,
            generation.Topics.Count,
            hourlyTopics.Count,
            generation.InputTokens,
            generation.OutputTokens,
            generation.CostUsd);
    }
}
