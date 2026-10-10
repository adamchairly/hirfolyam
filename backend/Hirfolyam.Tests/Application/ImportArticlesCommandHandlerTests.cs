using Hirfolyam.Application.Features;
using Hirfolyam.Application.Options;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Hirfolyam.Tests.Application;

public sealed class ImportArticlesCommandHandlerTests
{
    private const string FeedUrl = "https://example.com/feed";
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-06T22:05:00Z");

    private string databaseName = null!;
    private NewsSourceCatalog catalog = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [TestCase(48, new[] { "yesterday", "recent", "now" })]
    [TestCase(24, new[] { "recent", "now" })]
    public async Task ImportUsesTheConfiguredLookbackAndSkipsFutureArticles(int lookbackHours, string[] expected)
    {
        await SeedDefaultSourcesAsync();
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] =
            [
                CreateArticle("too-old", Now.AddHours(-49)),
                CreateArticle("yesterday", Now.AddHours(-25)),
                CreateArticle("recent", Now.AddHours(-1)),
                CreateArticle("now", Now),
                CreateArticle("future", Now.AddHours(1)),
                CreateArticle("missing-date", null)
            ]
        });
        var importer = new StubArticleImporter();

        var result = await HandleAsync(feeds, importer, lookbackHours: lookbackHours);

        Assert.That(importer.Articles.Select(article => article.ExternalId), Is.EqualTo(expected));
        Assert.That(result.Imported, Is.EqualTo(expected.Length));
        Assert.That(result.Skipped, Is.EqualTo(6 - expected.Length));
    }

    [Test]
    public async Task ImportContinuesAfterFailuresAndCountsDuplicatesSeparately()
    {
        const string secondFeed = "https://example.com/second";
        await SeedDefaultSourcesAsync(["https://example.com/broken", FeedUrl, secondFeed]);
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] =
            [
                CreateArticle("failed", DateTimeOffset.Parse("2026-10-06T22:01:00Z")),
                CreateArticle("duplicate", DateTimeOffset.Parse("2026-10-06T22:02:00Z"))
            ],
            [secondFeed] = [CreateArticle("success", DateTimeOffset.Parse("2026-10-06T22:03:00Z"))]
        });
        var importer = new StubArticleImporter
        {
            Import = article => article.ExternalId switch
            {
                "failed" => throw new InvalidDataException("The article is invalid."),
                "duplicate" => false,
                _ => true
            }
        };

        var result = await HandleAsync(feeds, importer);

        Assert.That(result.Imported, Is.EqualTo(1));
        Assert.That(result.Duplicates, Is.EqualTo(1));
        Assert.That(result.FailedArticles, Is.EqualTo(1));
        Assert.That(result.FailedFeeds, Is.EqualTo(1));
        Assert.That(result.Skipped, Is.Zero);
        Assert.That(importer.Articles.Select(article => article.ExternalId),
            Is.EqualTo(new[] { "failed", "duplicate", "success" }));
    }

    [Test]
    public async Task DisabledSourcesAreSkippedAndStoredIdsArePassedOn()
    {
        await SeedDefaultSourcesAsync();
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] = [CreateArticle("article", DateTimeOffset.Parse("2026-10-06T22:01:00Z"))]
        });
        var importer = new StubArticleImporter();

        var result = await HandleAsync(feeds, importer);

        Assert.That(feeds.Calls, Is.EqualTo(new[] { FeedUrl }));
        Assert.That(importer.Sources, Is.EqualTo(new[] { (7, "news") }));
        Assert.That(result.Imported, Is.EqualTo(1));
        Assert.That(result.FailedFeeds, Is.Zero);
    }

    [Test]
    public async Task SourceMissingFromTheDatabaseIsReportedAsAFailedFeed()
    {
        catalog = CreateCatalog(CreateDefinition("news", enabled: true, [FeedUrl]));
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] = [CreateArticle("article", DateTimeOffset.Parse("2026-10-06T22:01:00Z"))]
        });

        var result = await HandleAsync(feeds, new StubArticleImporter());

        Assert.That(result.FailedFeeds, Is.EqualTo(1));
        Assert.That(feeds.Calls, Is.Empty);
    }

    [Test]
    public async Task ASourceRecordsItsImportTimeWhenEveryFeedWasRead()
    {
        await SeedDefaultSourcesAsync();
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] = []
        });

        await HandleAsync(feeds, new StubArticleImporter());
        var sources = await ReadSourcesAsync();

        Assert.That(sources.Single(source => source.Key == "news").LastImportedAt, Is.EqualTo(Now));
        Assert.That(sources.Single(source => source.Key == "disabled").LastImportedAt, Is.Null);
    }

    [Test]
    public async Task AFailedFeedKeepsThePreviousImportTimeOfItsSource()
    {
        await SeedDefaultSourcesAsync([FeedUrl, "https://example.com/broken"]);
        var feeds = new StubFeedReader(new Dictionary<string, IReadOnlyCollection<RssArticle>>
        {
            [FeedUrl] = []
        });

        var result = await HandleAsync(feeds, new StubArticleImporter());
        var sources = await ReadSourcesAsync();

        Assert.That(result.FailedFeeds, Is.EqualTo(1));
        Assert.That(sources.Single(source => source.Key == "news").LastImportedAt, Is.Null);
    }

    private async Task<ImportArticlesResponse> HandleAsync(
        StubFeedReader feeds,
        StubArticleImporter importer,
        int lookbackHours = 48,
        CancellationToken cancellationToken = default)
    {
        var clock = new FixedTimeProvider(Now);
        var importOptions = OptionsFactory.Create(new ImportOptions
        {
            LookbackHours = lookbackHours
        });
        await using var context = TestDatabase.Create(databaseName);
        var handler = new ImportArticlesCommandHandler(
            context,
            catalog,
            feeds,
            importer,
            clock,
            importOptions,
            NullLogger<ImportArticlesCommandHandler>.Instance);

        return await handler.Handle(new ImportArticlesCommand(), cancellationToken);
    }

    private async Task SeedDefaultSourcesAsync(string[]? feedUrls = null)
    {
        catalog = CreateCatalog(
            CreateDefinition("news", enabled: true, feedUrls ?? [FeedUrl]),
            CreateDefinition("disabled", enabled: false, ["https://example.com/disabled"]));

        await using var context = TestDatabase.Create(databaseName);
        context.NewsSources.AddRange(
            new NewsSource { Id = 7, Key = "news", Name = "news", BaseUrl = "https://example.com/" },
            new NewsSource { Id = 8, Key = "disabled", Name = "disabled", BaseUrl = "https://example.com/" });
        await context.SaveChangesAsync();
    }

    private static NewsSourceCatalog CreateCatalog(params NewsSourceDefinition[] sources)
    {
        return new NewsSourceCatalog(sources);
    }

    private static NewsSourceDefinition CreateDefinition(string key, bool enabled, string[] feeds)
    {
        return new NewsSourceDefinition
        {
            Key = key,
            Name = key,
            BaseUrl = "https://example.com/",
            Enabled = enabled,
            Feeds = feeds
        };
    }

    private async Task<List<NewsSource>> ReadSourcesAsync()
    {
        await using var context = TestDatabase.Create(databaseName);

        return await context.NewsSources.AsNoTracking().ToListAsync();
    }

    private static RssArticle CreateArticle(string id, DateTimeOffset? publishedAt)
    {
        return new RssArticle
        {
            ExternalId = id,
            Title = id,
            Url = "https://example.com/" + id,
            PublishedAt = publishedAt
        };
    }

    private sealed class StubArticleImporter : IArticleImporter
    {
        public Func<RssArticle, bool> Import { get; init; } = _ => true;

        public List<RssArticle> Articles { get; } = [];

        public List<(int Id, string Key)> Sources { get; } = [];

        public Task<bool> ImportAsync(
            int sourceId,
            string sourceKey,
            RssArticle article,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Articles.Add(article);
            Sources.Add((sourceId, sourceKey));

            return Task.FromResult(Import(article));
        }
    }
}
