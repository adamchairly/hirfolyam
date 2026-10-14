using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Hirfolyam.Application.Shared;
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

    [Test]
    public async Task SummariesOfADayAreNotShownAsHourlyTopics()
    {
        await SeedAsync(
            [CreateArticle(1, 1, Now), CreateArticle(2, 2, Now)],
            CreateSnapshot(Now.AddHours(-1), CreateTopic(1, "Hourly", [1, 2])),
            CreateDailySnapshot(new DateOnly(2026, 10, 10), Now.AddMinutes(-5), CreateTopic(1, "Daily", [1, 2])));

        var result = await HandleAsync();

        Assert.That(result.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "Hourly" }));
    }

    [Test]
    public async Task TodayAndYesterdayShowTheNewestSummaryOfTheirDay()
    {
        await SeedAsync(
            [CreateArticle(1, 1, Now), CreateArticle(2, 2, Now)],
            CreateSnapshot(Now.AddMinutes(-5), CreateTopic(1, "Hourly", [1, 2])),
            CreateDailySnapshot(new DateOnly(2026, 10, 10), Now.AddHours(-2), CreateTopic(1, "Noon", [1, 2])),
            CreateDailySnapshot(new DateOnly(2026, 10, 10), Now.AddHours(-1), CreateTopic(1, "Afternoon", [1, 2])),
            CreateDailySnapshot(new DateOnly(2026, 10, 9), Now.AddHours(-14), CreateTopic(1, "Yesterday", [1, 2])),
            CreateDailySnapshot(new DateOnly(2026, 10, 8), Now.AddHours(-38), CreateTopic(1, "Older", [1, 2])));

        var today = await HandleAsync(HotTopicPeriod.Today);
        var yesterday = await HandleAsync(HotTopicPeriod.Yesterday);

        Assert.That(today.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "Afternoon" }));
        Assert.That(today.GeneratedAt, Is.EqualTo(Now.AddHours(-1)));
        Assert.That(today.Items[0].Articles.Select(article => article.Id), Is.EqualTo(new long[] { 2, 1 }));
        Assert.That(yesterday.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "Yesterday" }));
    }

    [Test]
    public async Task TheDaysFollowTheBudapestCalendar()
    {
        // Half past midnight in Budapest, still the previous day in UTC.
        var now = DateTimeOffset.Parse("2026-10-10T22:30:00Z");
        await SeedAsync(
            [CreateArticle(1, 1, Now), CreateArticle(2, 2, Now)],
            CreateDailySnapshot(new DateOnly(2026, 10, 10), Now.AddHours(8), CreateTopic(1, "Evening", [1, 2])));

        var today = await HandleAsync(HotTopicPeriod.Today, now);
        var yesterday = await HandleAsync(HotTopicPeriod.Yesterday, now);

        Assert.That(today.Items, Is.Empty);
        Assert.That(today.GeneratedAt, Is.Null);
        Assert.That(yesterday.Items.Select(topic => topic.Title), Is.EqualTo(new[] { "Evening" }));
    }

    [Test]
    public async Task ATopicIsReturnedByItsIdWithTheListedArticlesNewestFirst()
    {
        await SeedAsync(
            [
                CreateArticle(1, 1, Now.AddHours(-2)),
                CreateArticle(2, 2, Now.AddHours(-1)),
                CreateArticle(3, 1, Now.AddHours(-3))
            ],
            CreateSnapshot(Now.AddMinutes(-10), CreateTopic(1, "First", [1, 2, 99])));

        var id = (await HandleAsync()).Items.Single().Id;
        var topic = await HandleTopicAsync(id);

        Assert.That(topic.Id, Is.EqualTo(id));
        Assert.That(topic.Title, Is.EqualTo("First"));
        Assert.That(topic.Summary, Is.EqualTo("First summary."));
        Assert.That(topic.GeneratedAt, Is.EqualTo(Now.AddMinutes(-10)));
        Assert.That(topic.Articles.Select(article => article.Id), Is.EqualTo(new long[] { 2, 1 }));
        Assert.That(topic.Articles[0].Source.Key, Is.EqualTo("index"));
    }

    [Test]
    public async Task ExpiredTopicsCanStillBeOpened()
    {
        var old = CreateTopic(1, "Old", [1]);
        old.Id = 5;
        await SeedAsync([CreateArticle(1, 1, Now)], CreateSnapshot(Now.AddDays(-2), old));

        var topic = await HandleTopicAsync(5);

        Assert.That(topic.Title, Is.EqualTo("Old"));
    }

    [Test]
    public async Task AnUnknownTopicIsNotFound()
    {
        await SeedAsync([CreateArticle(1, 1, Now)], CreateSnapshot(Now, CreateTopic(1, "Topic", [1])));

        Assert.That(() => HandleTopicAsync(42), Throws.TypeOf<NotFoundException>());
    }

    private async Task<GetHotTopicResponse> HandleTopicAsync(long id)
    {
        await using var context = TestDatabase.Create(databaseName);
        var handler = new GetHotTopicQueryHandler(context);

        return await handler.Handle(new GetHotTopicQuery { Id = id }, CancellationToken.None);
    }

    private async Task<GetHotTopicsResponse> HandleAsync(
        HotTopicPeriod period = HotTopicPeriod.Hour,
        DateTimeOffset? now = null)
    {
        await using var context = TestDatabase.Create(databaseName);
        var handler = new GetHotTopicsQueryHandler(
            context,
            OptionsFactory.Create(new HotTopicsOptions { MaxAgeHours = 3 }),
            new FixedTimeProvider(now ?? Now));

        return await handler.Handle(new GetHotTopicsQuery { Period = period }, CancellationToken.None);
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

    private static HotTopicSnapshot CreateDailySnapshot(DateOnly day, DateTimeOffset generatedAt, params HotTopic[] topics)
    {
        var snapshot = CreateSnapshot(generatedAt, topics);
        snapshot.Day = day;

        return snapshot;
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
