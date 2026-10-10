using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Tests.Support;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Hirfolyam.Tests.Application;

public sealed class GetHotTopicsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-10T12:00:00Z");

    private string databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [Test]
    public async Task TopicsAreReturnedInRankWithTheirArticlesNewestFirst()
    {
        await SeedAsync(
            [
                CreateArticle(1, 1, Now.AddHours(-2)),
                CreateArticle(2, 2, Now.AddHours(-1)),
                CreateArticle(3, 1, Now.AddHours(-3))
            ],
            CreateSnapshot(
                Now.AddMinutes(-10),
                CreateTopic(2, "Second", [3, 1]),
                CreateTopic(1, "First", [1, 2])));

        var result = await HandleAsync();

        Assert.That(result.GeneratedAt, Is.EqualTo(Now.AddMinutes(-10)));
        Assert.That(result.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "First", "Second" }));
        Assert.That(result.Items[0].Summary, Is.EqualTo("First summary."));
        Assert.That(result.Items[0].Articles.Select(article => article.Id), Is.EqualTo(new long[] { 2, 1 }));

        var article = result.Items[0].Articles[0];
        Assert.That(article.Title, Is.EqualTo("Title 2"));
        Assert.That(article.Url, Is.EqualTo("https://index.hu/2"));
        Assert.That(article.Source.Key, Is.EqualTo("index"));
        Assert.That(article.Source.Name, Is.EqualTo("Index"));
        Assert.That(article.Source.IconUrl, Is.EqualTo("https://index.hu/icon.png"));
    }

    [Test]
    public async Task TheNewestSnapshotReplacesTheOlderTopics()
    {
        await SeedAsync(
            [CreateArticle(1, 1, Now), CreateArticle(2, 2, Now)],
            CreateSnapshot(Now.AddHours(-2), CreateTopic(1, "Older", [1, 2])),
            CreateSnapshot(Now.AddHours(-1), CreateTopic(1, "Newer", [1, 2])));

        var result = await HandleAsync();

        Assert.That(result.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "Newer" }));
    }

    [Test]
    public async Task AnEmptyNewestSnapshotHidesTheOlderTopics()
    {
        await SeedAsync(
            [CreateArticle(1, 1, Now), CreateArticle(2, 2, Now)],
            CreateSnapshot(Now.AddHours(-1), CreateTopic(1, "Older", [1, 2])),
            CreateSnapshot(Now.AddMinutes(-5)));

        var result = await HandleAsync();

        Assert.That(result.GeneratedAt, Is.EqualTo(Now.AddMinutes(-5)));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task MissingArticlesAreSkipped()
    {
        await SeedAsync([CreateArticle(1, 1, Now)], CreateSnapshot(Now, CreateTopic(1, "Topic", [1, 99])));

        var result = await HandleAsync();

        Assert.That(result.Items.Single().Articles.Select(article => article.Id), Is.EqualTo(new long[] { 1 }));
    }

    [Test]
    public async Task ExpiredSnapshotsAreNotReturned()
    {
        await SeedAsync(
            [CreateArticle(1, 1, Now)],
            CreateSnapshot(Now.AddHours(-3).AddMinutes(-1), CreateTopic(1, "Old", [1])));

        var result = await HandleAsync();

        Assert.That(result.Items, Is.Empty);
        Assert.That(result.GeneratedAt, Is.Null);
    }

    private async Task<GetHotTopicsResponse> HandleAsync()
    {
        await using var context = TestDatabase.Create(databaseName);
        var handler = new GetHotTopicsQueryHandler(
            context,
            OptionsFactory.Create(new HotTopicsOptions { MaxAgeHours = 3 }),
            new FixedTimeProvider(Now));

        return await handler.Handle(new GetHotTopicsQuery(), CancellationToken.None);
    }

    private async Task SeedAsync(Article[] articles, params HotTopicSnapshot[] snapshots)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.NewsSources.AddRange(CreateSource(1, "telex", "Telex"), CreateSource(2, "index", "Index"));
        context.Articles.AddRange(articles);
        context.HotTopicSnapshots.AddRange(snapshots);
        await context.SaveChangesAsync();
    }

    private static NewsSource CreateSource(int id, string key, string name)
    {
        return new NewsSource
        {
            Id = id,
            Key = key,
            Name = name,
            BaseUrl = $"https://{key}.hu",
            IconUrl = $"https://{key}.hu/icon.png"
        };
    }

    private static Article CreateArticle(long id, int sourceId, DateTimeOffset publishedAt)
    {
        var key = sourceId == 1 ? "telex" : "index";

        return new Article
        {
            Id = id,
            NewsSourceId = sourceId,
            ExternalId = id.ToString(),
            ExternalIdHash = id.ToString(),
            Url = $"https://{key}.hu/{id}",
            UrlHash = id.ToString(),
            Title = $"Title {id}",
            PublishedAt = publishedAt,
            CreatedAt = publishedAt,
            UpdatedAt = publishedAt
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

    private static HotTopic CreateTopic(int rank, string title, long[] articleIds)
    {
        return new HotTopic
        {
            Rank = rank,
            Title = title,
            Summary = $"{title} summary.",
            ArticleIds = articleIds
        };
    }
}
