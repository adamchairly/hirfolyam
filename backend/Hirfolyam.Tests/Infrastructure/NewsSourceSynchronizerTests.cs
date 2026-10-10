using Hirfolyam.Application.Features;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Infrastructure.StartupTasks;
using Hirfolyam.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hirfolyam.Tests.Infrastructure;

public sealed class NewsSourceSynchronizerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-06T12:00:00Z");
    private static readonly DateTimeOffset Earlier = Now.AddDays(-1);

    private string databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [Test]
    public async Task EveryDefinitionIsStoredOnTheFirstRun()
    {
        var result = await HandleAsync(NewsSourceCatalog.Load());
        var sources = await ReadSourcesAsync();
        var telex = sources.Single(source => source.Key == "telex");

        Assert.That(result.Added, Is.EqualTo(46));
        Assert.That(sources, Has.Count.EqualTo(46));
        Assert.That(telex.Name, Is.EqualTo("Telex"));
        Assert.That(telex.BaseUrl, Is.EqualTo("https://telex.hu/"));
        Assert.That(telex.IsEnabled, Is.True);
        Assert.That(telex.CreatedAt, Is.EqualTo(Now));
    }

    [Test]
    public async Task ChangedDefinitionsUpdateTheStoredSourceAndKeepItsId()
    {
        await SeedAsync(CreateStoredSource(id: 5, key: "news", name: "Old name"));

        var result = await HandleAsync(CreateCatalog(CreateDefinition("news", name: "New name")));
        var source = (await ReadSourcesAsync()).Single();

        Assert.That(result.Updated, Is.EqualTo(1));
        Assert.That(source.Id, Is.EqualTo(5));
        Assert.That(source.Name, Is.EqualTo("New name"));
        Assert.That(source.CreatedAt, Is.EqualTo(Earlier));
        Assert.That(source.UpdatedAt, Is.EqualTo(Now));
    }

    [Test]
    public async Task UnchangedDefinitionsAreLeftAlone()
    {
        await SeedAsync(CreateStoredSource(id: 5, key: "news", name: "News"));

        var result = await HandleAsync(CreateCatalog(CreateDefinition("news", name: "News")));

        Assert.That(result.Updated, Is.Zero);
        Assert.That((await ReadSourcesAsync()).Single().UpdatedAt, Is.EqualTo(Earlier));
    }

    [Test]
    public async Task SourcesRemovedFromTheCatalogAreDisabled()
    {
        await SeedAsync(CreateStoredSource(id: 5, key: "removed", name: "Removed"));

        var result = await HandleAsync(CreateCatalog());
        var source = (await ReadSourcesAsync()).Single();

        Assert.That(result.Disabled, Is.EqualTo(1));
        Assert.That(source.IsEnabled, Is.False);
        Assert.That(source.UpdatedAt, Is.EqualTo(Now));
    }

    private async Task<NewsSourceSyncResult> HandleAsync(NewsSourceCatalog catalog)
    {
        await using var database = TestDatabase.Create(databaseName);
        var synchronizer = new NewsSourceSynchronizer(
            database,
            catalog,
            new FixedTimeProvider(Now),
            NullLogger<NewsSourceSynchronizer>.Instance);

        return await synchronizer.SynchronizeAsync(CancellationToken.None);
    }

    private async Task SeedAsync(NewsSource source)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.NewsSources.Add(source);
        await context.SaveChangesAsync();
    }

    private async Task<List<NewsSource>> ReadSourcesAsync()
    {
        await using var context = TestDatabase.Create(databaseName);

        return await context.NewsSources.AsNoTracking().ToListAsync();
    }

    private static NewsSourceCatalog CreateCatalog(params NewsSourceDefinition[] sources)
    {
        return new NewsSourceCatalog(sources);
    }

    private static NewsSourceDefinition CreateDefinition(string key, string name)
    {
        return new NewsSourceDefinition
        {
            Key = key,
            Name = name,
            BaseUrl = "https://example.com/",
            Feeds = ["https://example.com/feed"]
        };
    }

    private static NewsSource CreateStoredSource(int id, string key, string name)
    {
        return new NewsSource
        {
            Id = id,
            Key = key,
            Name = name,
            BaseUrl = "https://example.com/",
            IsEnabled = true,
            CreatedAt = Earlier,
            UpdatedAt = Earlier
        };
    }
}
