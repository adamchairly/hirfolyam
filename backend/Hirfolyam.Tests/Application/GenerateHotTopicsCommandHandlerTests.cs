using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Hirfolyam.Tests.Application;

public sealed class GenerateHotTopicsCommandHandlerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-10T12:00:00Z");
    private static readonly NewsSource Telex = CreateSource(1, "telex", "Telex");
    private static readonly NewsSource Index = CreateSource(2, "index", "Index");
    private static readonly NewsSource Hvg = CreateSource(3, "hvg", "HVG");

    private string databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [Test]
    public async Task TheNewestArticlesAreSentWithTheirSourceAndPlainLead()
    {
        await SeedAsync(
            CreateArticle(1, Telex, Now.AddHours(-3)),
            CreateArticle(2, Index, Now.AddHours(-1)),
            CreateArticle(3, Telex, Now.AddHours(-2)));
        var generator = new StubGenerator();

        await HandleAsync(generator, articleCount: 2);

        Assert.That(generator.Articles.Select(article => article.Id), Is.EqualTo(new long[] { 2, 3 }));
        Assert.That(generator.Articles[0].Source, Is.EqualTo("Index"));
        Assert.That(generator.Articles[0].Title, Is.EqualTo("Title 2"));
        Assert.That(generator.Articles[0].Lead, Is.EqualTo("Plain lead 2"));
        Assert.That(generator.TopicCount, Is.EqualTo(5));
    }

    [Test]
    public async Task TopicsNeedTwoSourcesAndAreRankedBySourcesThenArticles()
    {
        await SeedAsync(
            CreateArticle(1, Telex, Now),
            CreateArticle(2, Index, Now),
            CreateArticle(3, Hvg, Now),
            CreateArticle(4, Telex, Now),
            CreateArticle(5, Index, Now),
            CreateArticle(6, Telex, Now),
            CreateArticle(7, Telex, Now),
            CreateArticle(8, Index, Now));
        var generator = new StubGenerator
        {
            Topics =
            [
                new HotTopicDraft("Two sources", "Two sources summary.", [4, 5]),
                new HotTopicDraft("One source", "One source summary.", [6, 7]),
                new HotTopicDraft("Three sources", "Three sources summary.", [1, 2, 3]),
                new HotTopicDraft("Two sources, more articles", "Summary.", [6, 7, 99, 5, 8])
            ]
        };

        var result = await HandleAsync(generator);
        var topics = await ReadTopicsAsync();

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Topics, Is.EqualTo(3));
        Assert.That(topics.Select(topic => topic.Title), Is.EqualTo(new[]
        {
            "Three sources",
            "Two sources, more articles",
            "Two sources"
        }));
        Assert.That(topics.Select(topic => topic.Rank), Is.EqualTo(new[] { 1, 2, 3 }));
        // The article 5 already belongs to an earlier topic and 99 does not exist.
        Assert.That(topics[1].ArticleIds, Is.EqualTo(new long[] { 6, 7, 8 }));
    }

    [Test]
    public async Task ArticlesOfARejectedTopicCanStillBelongToALaterTopic()
    {
        await SeedAsync(
            CreateArticle(1, Telex, Now),
            CreateArticle(2, Telex, Now),
            CreateArticle(3, Index, Now));
        var generator = new StubGenerator
        {
            Topics =
            [
                new HotTopicDraft("One source", "Summary.", [1, 2]),
                new HotTopicDraft("Two sources", "Summary.", [1, 3])
            ]
        };

        await HandleAsync(generator);
        var topic = (await ReadTopicsAsync()).Single();

        Assert.That(topic.Title, Is.EqualTo("Two sources"));
        Assert.That(topic.ArticleIds, Is.EqualTo(new long[] { 1, 3 }));
    }

    [Test]
    public async Task OnlyTheConfiguredNumberOfTopicsIsKept()
    {
        await SeedAsync(
            CreateArticle(1, Telex, Now),
            CreateArticle(2, Index, Now),
            CreateArticle(3, Telex, Now),
            CreateArticle(4, Index, Now));
        var generator = new StubGenerator
        {
            Topics =
            [
                new HotTopicDraft("First", "Summary.", [1, 2]),
                new HotTopicDraft("Second", "Summary.", [3, 4])
            ]
        };

        await HandleAsync(generator, topicCount: 1);

        Assert.That((await ReadTopicsAsync()).Select(topic => topic.Title), Is.EqualTo(new[] { "First" }));
    }

    [Test]
    public async Task TopicsWithoutTextOrWithTooLongTextAreDropped()
    {
        await SeedAsync(
            CreateArticle(1, Telex, Now),
            CreateArticle(2, Index, Now),
            CreateArticle(3, Telex, Now),
            CreateArticle(4, Index, Now),
            CreateArticle(5, Telex, Now),
            CreateArticle(6, Index, Now));
        var generator = new StubGenerator
        {
            Topics =
            [
                new HotTopicDraft(" ", "Summary.", [1, 2]),
                new HotTopicDraft("Title", new string('a', HotTopic.SummaryMaxLength + 1), [3, 4]),
                new HotTopicDraft("  Kept  ", "  Summary.  ", [5, 6])
            ]
        };

        await HandleAsync(generator);
        var topic = (await ReadTopicsAsync()).Single();

        Assert.That(topic.Title, Is.EqualTo("Kept"));
        Assert.That(topic.Summary, Is.EqualTo("Summary."));
    }

    [Test]
    public async Task ASuccessfulRunAddsASnapshotAndKeepsTheOlderOnes()
    {
        await SeedAsync(CreateArticle(1, Telex, Now), CreateArticle(2, Index, Now));
        await SeedSnapshotAsync(CreateSnapshot("Old", Now.AddHours(-1)));
        var generator = new StubGenerator
        {
            Topics = [new HotTopicDraft("New", "Summary.", [1, 2])],
            InputTokens = 11000,
            OutputTokens = 700,
            CostUsd = 0.0017m
        };

        await HandleAsync(generator);
        var snapshots = await ReadSnapshotsAsync();
        var latest = snapshots[^1];

        Assert.That(snapshots.Select(snapshot => snapshot.Topics.Single().Title), Is.EqualTo(new[] { "Old", "New" }));
        Assert.That(latest.GeneratedAt, Is.EqualTo(Now));
        Assert.That(latest.Model, Is.EqualTo("openai/gpt-6-luna"));
        Assert.That(latest.ArticleCount, Is.EqualTo(2));
        Assert.That(latest.InputTokens, Is.EqualTo(11000));
        Assert.That(latest.OutputTokens, Is.EqualTo(700));
        Assert.That(latest.CostUsd, Is.EqualTo(0.0017m));
    }

    [Test]
    public async Task ARunWithoutQualifyingTopicsStoresAnEmptySnapshot()
    {
        await SeedAsync(CreateArticle(1, Telex, Now), CreateArticle(2, Telex, Now));
        var generator = new StubGenerator
        {
            Topics = [new HotTopicDraft("One source", "Summary.", [1, 2])]
        };

        var result = await HandleAsync(generator);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Topics, Is.Zero);
        Assert.That((await ReadSnapshotsAsync()).Single().Topics, Is.Empty);
    }

    [Test]
    public async Task AFailedRunStoresNoSnapshot()
    {
        await SeedAsync(CreateArticle(1, Telex, Now), CreateArticle(2, Index, Now));
        await SeedSnapshotAsync(CreateSnapshot("Old", Now.AddHours(-1)));
        var generator = new StubGenerator
        {
            Failure = new HttpRequestException("Service unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable)
        };

        var result = await HandleAsync(generator);

        Assert.That(result.Succeeded, Is.False);
        Assert.That((await ReadSnapshotsAsync()).Single().Topics.Single().Title, Is.EqualTo("Old"));
    }

    [Test]
    public async Task WithoutArticlesTheGeneratorIsNotCalled()
    {
        var generator = new StubGenerator();

        var result = await HandleAsync(generator);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(generator.Articles, Is.Empty);
        Assert.That(await ReadSnapshotsAsync(), Is.Empty);
    }

    private async Task<GenerateHotTopicsResponse> HandleAsync(
        StubGenerator generator,
        int articleCount = 100,
        int topicCount = 5)
    {
        await using var context = TestDatabase.Create(databaseName);
        var handler = new GenerateHotTopicsCommandHandler(
            context,
            generator,
            OptionsFactory.Create(new HotTopicsOptions
            {
                ArticleCount = articleCount,
                TopicCount = topicCount
            }),
            new FixedTimeProvider(Now),
            NullLogger<GenerateHotTopicsCommandHandler>.Instance);

        return await handler.Handle(new GenerateHotTopicsCommand(), CancellationToken.None);
    }

    private async Task SeedAsync(params Article[] articles)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.NewsSources.AddRange(CreateSource(1, "telex", "Telex"), CreateSource(2, "index", "Index"), CreateSource(3, "hvg", "HVG"));
        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();
    }

    private async Task SeedSnapshotAsync(HotTopicSnapshot snapshot)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.HotTopicSnapshots.Add(snapshot);
        await context.SaveChangesAsync();
    }

    private async Task<List<HotTopicSnapshot>> ReadSnapshotsAsync()
    {
        await using var context = TestDatabase.Create(databaseName);

        return await context.HotTopicSnapshots
            .AsNoTracking()
            .Include(snapshot => snapshot.Topics)
            .OrderBy(snapshot => snapshot.GeneratedAt)
            .ToListAsync();
    }

    // The topics of the newest snapshot, in rank order.
    private async Task<List<HotTopic>> ReadTopicsAsync()
    {
        var latest = (await ReadSnapshotsAsync())[^1];

        return latest.Topics.OrderBy(topic => topic.Rank).ToList();
    }

    private static NewsSource CreateSource(int id, string key, string name)
    {
        return new NewsSource
        {
            Id = id,
            Key = key,
            Name = name,
            BaseUrl = $"https://{key}.hu"
        };
    }

    private static Article CreateArticle(long id, NewsSource source, DateTimeOffset publishedAt)
    {
        return new Article
        {
            Id = id,
            NewsSourceId = source.Id,
            ExternalId = id.ToString(),
            ExternalIdHash = id.ToString(),
            Url = $"https://{source.Key}.hu/{id}",
            UrlHash = id.ToString(),
            Title = $"Title {id}",
            Lead = $"<p>Plain lead {id}</p>",
            LeadSearchText = $"Plain lead {id}",
            PublishedAt = publishedAt,
            CreatedAt = publishedAt,
            UpdatedAt = publishedAt
        };
    }

    private static HotTopicSnapshot CreateSnapshot(string title, DateTimeOffset generatedAt)
    {
        return new HotTopicSnapshot
        {
            GeneratedAt = generatedAt,
            Model = "openai/gpt-6-luna",
            ArticleCount = 2,
            Topics =
            [
                new HotTopic
                {
                    Rank = 1,
                    Title = title,
                    Summary = "Summary.",
                    ArticleIds = [1, 2]
                }
            ]
        };
    }

    private sealed class StubGenerator : IHotTopicGenerator
    {
        public string Model => "openai/gpt-6-luna";

        public IReadOnlyList<HotTopicDraft> Topics { get; init; } = [];

        public int? InputTokens { get; init; }

        public int? OutputTokens { get; init; }

        public decimal? CostUsd { get; init; }

        public Exception? Failure { get; init; }

        public IReadOnlyList<HotTopicArticle> Articles { get; private set; } = [];

        public int TopicCount { get; private set; }

        public Task<HotTopicGeneration> GenerateAsync(
            IReadOnlyList<HotTopicArticle> articles,
            int topicCount,
            CancellationToken cancellationToken)
        {
            Articles = articles;
            TopicCount = topicCount;

            if (Failure is not null)
            {
                throw Failure;
            }

            return Task.FromResult(new HotTopicGeneration(Topics, InputTokens, OutputTokens, CostUsd));
        }
    }
}
