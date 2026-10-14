using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Hirfolyam.Tests.Application;

public sealed class GenerateDailyTopicsCommandHandlerTests
{
    // 14:00 in Budapest; the day started at 22:00 UTC the evening before.
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-10T12:00:00Z");
    private static readonly DateOnly Today = new(2026, 10, 10);
    private static readonly DateOnly Yesterday = new(2026, 10, 9);

    private string databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [Test]
    public async Task TheHourlyTopicsOfTheBudapestDayAreSentInTimeAndRankOrder()
    {
        await SeedAsync(
            [],
            CreateDailySnapshot(Yesterday, Now.AddHours(-20)),
            CreateSnapshot(DateTimeOffset.Parse("2026-10-09T21:30:00Z"), CreateTopic(1, 1, "Before midnight", [1, 2])),
            CreateSnapshot(Now.AddHours(-2), CreateTopic(3, 1, "Third", [1, 2])),
            CreateSnapshot(
                DateTimeOffset.Parse("2026-10-09T22:30:00Z"),
                CreateTopic(4, 2, "Second", [1, 2]),
                CreateTopic(5, 1, "First", [1, 2])),
            CreateDailySnapshot(Today, Now.AddHours(-1), CreateTopic(6, 1, "Earlier summary", [1, 2])));
        var generator = new StubGenerator();

        await HandleAsync(generator);
        var call = generator.Calls.Single();

        Assert.That(call.Select(topic => topic.Title), Is.EqualTo(new[] { "First", "Second", "Third" }));
        Assert.That(call.Select(topic => topic.Id), Is.EqualTo(new long[] { 5, 4, 3 }));
        Assert.That(call[0].Summary, Is.EqualTo("First summary."));
        Assert.That(generator.TopicCount, Is.EqualTo(5));
    }

    [Test]
    public async Task AStoryGetsTheArticlesOfItsTopicsAndTheStoriesAreRanked()
    {
        await SeedAsync(
            [
                CreateArticle(1, 1),
                CreateArticle(2, 2),
                CreateArticle(3, 3),
                CreateArticle(4, 1),
                CreateArticle(5, 2)
            ],
            CreateDailySnapshot(Yesterday, Now.AddHours(-20)),
            CreateSnapshot(Now.AddHours(-3), CreateTopic(11, 1, "Big, morning", [1, 2]), CreateTopic(12, 2, "Small", [4, 5])),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(21, 1, "Big, afternoon", [2, 3]), CreateTopic(22, 2, "Lonely", [4, 99])));
        var generator = new StubGenerator
        {
            Stories =
            [
                new DailyTopicDraft("Small story", "Small summary.", [12]),
                new DailyTopicDraft("Big story", "Big summary.", [11, 21, 999]),
                new DailyTopicDraft("Lonely story", "Lonely summary.", [22])
            ],
            InputTokens = 4000,
            OutputTokens = 500,
            CostUsd = 0.0008m
        };

        await HandleAsync(generator);
        var summary = (await ReadDailySnapshotsAsync(Today)).Single();
        var topics = summary.Topics.OrderBy(topic => topic.Rank).ToList();

        Assert.That(topics.Select(topic => topic.Title), Is.EqualTo(new[] { "Big story", "Small story" }));
        Assert.That(topics[0].Summary, Is.EqualTo("Big summary."));
        Assert.That(topics[0].ArticleIds, Is.EqualTo(new long[] { 1, 2, 3 }));
        Assert.That(topics[1].ArticleIds, Is.EqualTo(new long[] { 4, 5 }));
        Assert.That(summary.GeneratedAt, Is.EqualTo(Now));
        Assert.That(summary.Model, Is.EqualTo("openai/gpt-6-luna"));
        Assert.That(summary.ArticleCount, Is.EqualTo(5));
        Assert.That(summary.InputTokens, Is.EqualTo(4000));
        Assert.That(summary.OutputTokens, Is.EqualTo(500));
        Assert.That(summary.CostUsd, Is.EqualTo(0.0008m));
    }

    [Test]
    public async Task AMissingSummaryOfYesterdayIsMadeBeforeToday()
    {
        await SeedAsync(
            [CreateArticle(1, 1), CreateArticle(2, 2)],
            CreateSnapshot(Now.AddHours(-20), CreateTopic(1, 1, "Yesterday", [1, 2])),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(2, 1, "Today", [1, 2])));
        var generator = new StubGenerator();

        await HandleAsync(generator);

        Assert.That(generator.Calls.Select(call => call.Single().Title), Is.EqualTo(new[] { "Yesterday", "Today" }));
        Assert.That((await ReadDailySnapshotsAsync(Yesterday)).Single().Topics.Single().Title, Is.EqualTo("Story of Yesterday"));
        Assert.That((await ReadDailySnapshotsAsync(Today)).Single().Topics.Single().Title, Is.EqualTo("Story of Today"));
    }

    [Test]
    public async Task AnExistingSummaryOfYesterdayIsKept()
    {
        await SeedAsync(
            [CreateArticle(1, 1), CreateArticle(2, 2)],
            CreateDailySnapshot(Yesterday, Now.AddHours(-14)),
            CreateSnapshot(Now.AddHours(-20), CreateTopic(1, 1, "Yesterday", [1, 2])),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(2, 1, "Today", [1, 2])));
        var generator = new StubGenerator();

        await HandleAsync(generator);

        Assert.That(generator.Calls.Select(call => call.Single().Title), Is.EqualTo(new[] { "Today" }));
        Assert.That(await ReadDailySnapshotsAsync(Yesterday), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ALaterRunAddsANewerSummaryOfTheDay()
    {
        await SeedAsync(
            [CreateArticle(1, 1), CreateArticle(2, 2)],
            CreateDailySnapshot(Yesterday, Now.AddHours(-14)),
            CreateDailySnapshot(Today, Now.AddHours(-2), CreateTopic(1, 1, "Noon", [1, 2])),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(2, 1, "Today", [1, 2])));

        await HandleAsync(new StubGenerator());

        Assert.That(
            (await ReadDailySnapshotsAsync(Today)).Select(snapshot => snapshot.Topics.Single().Title),
            Is.EqualTo(new[] { "Noon", "Story of Today" }));
    }

    [Test]
    public async Task AFailedRunStoresNoSummary()
    {
        await SeedAsync(
            [CreateArticle(1, 1), CreateArticle(2, 2)],
            CreateDailySnapshot(Yesterday, Now.AddHours(-14)),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(1, 1, "Today", [1, 2])));
        var generator = new StubGenerator
        {
            Failure = new HttpRequestException("Service unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable)
        };

        await HandleAsync(generator);

        Assert.That(await ReadDailySnapshotsAsync(Today), Is.Empty);
    }

    [Test]
    public async Task WithoutHourlyTopicsTheGeneratorIsNotCalled()
    {
        await SeedAsync([], CreateSnapshot(Now.AddHours(-1)));
        var generator = new StubGenerator();

        await HandleAsync(generator);

        Assert.That(generator.Calls, Is.Empty);
        Assert.That(await ReadDailySnapshotsAsync(Yesterday), Is.Empty);
        Assert.That(await ReadDailySnapshotsAsync(Today), Is.Empty);
    }

    private async Task HandleAsync(StubGenerator generator)
    {
        await using var context = TestDatabase.Create(databaseName);
        var handler = new GenerateDailyTopicsCommandHandler(
            context,
            generator,
            OptionsFactory.Create(new HotTopicsOptions { TopicCount = 5 }),
            new FixedTimeProvider(Now),
            NullLogger<GenerateDailyTopicsCommandHandler>.Instance);

        await handler.Handle(new GenerateDailyTopicsCommand(), CancellationToken.None);
    }

    private async Task SeedAsync(Article[] articles, params HotTopicSnapshot[] snapshots)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.NewsSources.AddRange(CreateSource(1, "telex"), CreateSource(2, "index"), CreateSource(3, "hvg"));
        context.Articles.AddRange(articles);
        context.HotTopicSnapshots.AddRange(snapshots);
        await context.SaveChangesAsync();
    }

    private async Task<List<HotTopicSnapshot>> ReadDailySnapshotsAsync(DateOnly day)
    {
        await using var context = TestDatabase.Create(databaseName);

        return await context.HotTopicSnapshots
            .AsNoTracking()
            .Include(snapshot => snapshot.Topics)
            .Where(snapshot => snapshot.Day == day)
            .OrderBy(snapshot => snapshot.GeneratedAt)
            .ToListAsync();
    }

    private static NewsSource CreateSource(int id, string key)
    {
        return new NewsSource
        {
            Id = id,
            Key = key,
            Name = key,
            BaseUrl = $"https://{key}.hu"
        };
    }

    private static Article CreateArticle(long id, int sourceId)
    {
        return new Article
        {
            Id = id,
            NewsSourceId = sourceId,
            ExternalId = id.ToString(),
            ExternalIdHash = id.ToString(),
            Url = $"https://example.hu/{id}",
            UrlHash = id.ToString(),
            Title = $"Title {id}",
            PublishedAt = Now,
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }

    private static HotTopicSnapshot CreateSnapshot(DateTimeOffset generatedAt, params HotTopic[] topics)
    {
        return new HotTopicSnapshot
        {
            GeneratedAt = generatedAt,
            Model = "openai/gpt-6-luna",
            ArticleCount = 100,
            Topics = [.. topics]
        };
    }

    private static HotTopicSnapshot CreateDailySnapshot(DateOnly day, DateTimeOffset generatedAt, params HotTopic[] topics)
    {
        var snapshot = CreateSnapshot(generatedAt, topics);
        snapshot.Day = day;

        return snapshot;
    }

    private static HotTopic CreateTopic(long id, int rank, string title, long[] articleIds)
    {
        return new HotTopic
        {
            Id = id,
            Rank = rank,
            Title = title,
            Summary = $"{title} summary.",
            ArticleIds = articleIds
        };
    }

    private sealed class StubGenerator : IHotTopicGenerator
    {
        public string Model => "openai/gpt-6-luna";

        // Without stories, every call merges all of its topics into one story.
        public IReadOnlyList<DailyTopicDraft>? Stories { get; init; }

        public int? InputTokens { get; init; }

        public int? OutputTokens { get; init; }

        public decimal? CostUsd { get; init; }

        public Exception? Failure { get; init; }

        public List<IReadOnlyList<DailyTopicInput>> Calls { get; } = [];

        public int TopicCount { get; private set; }

        public Task<HotTopicGeneration> GenerateAsync(
            IReadOnlyList<HotTopicArticle> articles,
            int topicCount,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<DailyTopicGeneration> GenerateDailyAsync(
            IReadOnlyList<DailyTopicInput> topics,
            int topicCount,
            CancellationToken cancellationToken)
        {
            Calls.Add(topics);
            TopicCount = topicCount;

            if (Failure is not null)
            {
                throw Failure;
            }

            var stories = Stories ?? [new DailyTopicDraft($"Story of {topics[0].Title}", "Summary.", [.. topics.Select(topic => topic.Id)])];

            return Task.FromResult(new DailyTopicGeneration(stories, InputTokens, OutputTokens, CostUsd));
        }
    }
}
