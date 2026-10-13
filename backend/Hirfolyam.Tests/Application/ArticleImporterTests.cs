using Hirfolyam.Application.Features;
using Hirfolyam.Domain.Catalogs;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Infrastructure.Persistence;
using Hirfolyam.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Tests.Application;

public sealed class ArticleImporterTests
{
    private const int SourceId = 7;
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-06T12:00:00Z");
    private static readonly DateTimeOffset Earlier = Now.AddDays(-1);
    private static readonly NewsSourceCatalog Catalog = NewsSourceCatalog.Load();

    private string databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        databaseName = Guid.NewGuid().ToString();
    }

    [Test]
    public async Task ExistingArticleIsNotSavedOrChangedAgain()
    {
        var stored = CreateStoredArticle(categoryId: null);
        stored.ImageUrl = "https://example.com/stored.jpg";
        await SeedAsync(stored);

        var imported = await ImportAsync("news", CreateArticle());
        var article = (await ReadArticlesAsync()).Single();

        Assert.That(imported, Is.False);
        Assert.That(article.CategoryId, Is.Null);
        Assert.That(article.ImageUrl, Is.EqualTo("https://example.com/stored.jpg"));
        Assert.That(article.UpdatedAt, Is.EqualTo(Earlier));
    }

    [Test]
    public async Task MissingImageOfExistingArticleIsAddedFromTheFeed()
    {
        await SeedAsync(CreateStoredArticle(categoryId: null));

        var imported = await ImportAsync("news", CreateArticle());
        var article = (await ReadArticlesAsync()).Single();

        Assert.That(imported, Is.False);
        Assert.That(article.ImageUrl, Is.EqualTo("https://example.com/cover.jpg"));
        Assert.That(article.Title, Is.EqualTo("Stored title"));
        Assert.That(article.UpdatedAt, Is.EqualTo(Now));
    }

    [Test]
    public async Task ArticleWithTheSameUrlButAnotherExternalIdIsADuplicate()
    {
        await SeedAsync(CreateStoredArticle(categoryId: 1));

        var imported = await ImportAsync("news", CreateArticle() with
        {
            ExternalId = "another-id"
        });

        Assert.That(imported, Is.False);
        Assert.That(await ReadArticlesAsync(), Has.Count.EqualTo(1));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public async Task MissingExternalIdUsesTheUrlWithoutTrackingParameters(string? externalId)
    {
        await ImportAsync("news", CreateArticle() with
        {
            ExternalId = externalId
        });

        var article = (await ReadArticlesAsync()).Single();

        Assert.That(article.ExternalId, Is.EqualTo("https://example.com/article"));
        Assert.That(article.Url, Is.EqualTo(article.ExternalId));
        Assert.That(article.ExternalIdHash, Is.EqualTo(article.UrlHash));
    }

    [TestCase(null)]
    [TestCase("<p>Full &amp; unchanged content.</p>")]
    public async Task ImportPreservesFeedDataAndUsesUtcPublicationTime(string? content)
    {
        var input = CreateArticle() with
        {
            Content = content
        };

        var imported = await ImportAsync("news", input);
        var article = (await ReadArticlesAsync()).Single();

        Assert.That(imported, Is.True);
        Assert.That(article.NewsSourceId, Is.EqualTo(SourceId));
        Assert.That(article.ExternalId, Is.EqualTo("stable-id"));
        Assert.That(article.ExternalIdHash, Is.EqualTo(ArticleImporter.Hash("stable-id")));
        Assert.That(article.Url, Is.EqualTo("https://example.com/article"));
        Assert.That(article.UrlHash, Is.EqualTo(ArticleImporter.Hash("https://example.com/article")));
        Assert.That(article.Title, Is.EqualTo(input.Title));
        Assert.That(article.Lead, Is.EqualTo(input.Description));
        Assert.That(article.LeadText, Is.EqualTo("Feed summary & details."));
        Assert.That(article.Content, Is.EqualTo(content));
        Assert.That(article.Author, Is.EqualTo(input.Author));
        Assert.That(article.ImageUrl, Is.EqualTo(input.ImageUrl));
        Assert.That(article.CategoryId, Is.EqualTo(CategoryId("science-tech")));
        Assert.That(article.PublishedAt, Is.EqualTo(DateTimeOffset.Parse("2026-10-06T10:00:00Z")));
        Assert.That(article.PublishedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(article.CreatedAt, Is.EqualTo(Now));
        Assert.That(article.UpdatedAt, Is.EqualTo(Now));
    }

    private async Task<bool> ImportAsync(string sourceKey, RssArticle article)
    {
        await using var context = TestDatabase.Create(databaseName);
        var source = new NewsSource
        {
            Id = SourceId,
            Key = sourceKey,
            Name = sourceKey,
            BaseUrl = "https://example.com/"
        };

        return await CreateImporter(context).ImportAsync(source, article, CancellationToken.None);
    }

    private static ArticleImporter CreateImporter(NewsDbContext context)
    {
        return new ArticleImporter(context, new CategoryMapper(Catalog), new StubFeedReader([]), new FixedTimeProvider(Now));
    }

    private async Task SeedAsync(Article article)
    {
        await using var context = TestDatabase.Create(databaseName);
        context.Articles.Add(article);
        await context.SaveChangesAsync();
    }

    private async Task<List<Article>> ReadArticlesAsync()
    {
        await using var context = TestDatabase.Create(databaseName);

        return await context.Articles.AsNoTracking().OrderBy(article => article.Id).ToListAsync();
    }

    private static int CategoryId(string slug)
    {
        return Categories.All.Single(category => category.Slug == slug).Id;
    }

    private static Article CreateStoredArticle(int? categoryId)
    {
        return new Article
        {
            NewsSourceId = SourceId,
            CategoryId = categoryId,
            ExternalId = "stable-id",
            ExternalIdHash = ArticleImporter.Hash("stable-id"),
            Url = "https://example.com/article",
            UrlHash = ArticleImporter.Hash("https://example.com/article"),
            Title = "Stored title",
            PublishedAt = Earlier,
            CreatedAt = Earlier,
            UpdatedAt = Earlier
        };
    }

    private static RssArticle CreateArticle()
    {
        return new RssArticle
        {
            ExternalId = "  stable-id  ",
            Title = "Feed title",
            Url = "https://example.com/article?utm_source=feed",
            Description = "<p>Feed summary &amp; details.</p>",
            Author = "Feed author",
            ImageUrl = "https://example.com/cover.jpg",
            PublishedAt = DateTimeOffset.Parse("2026-10-06T12:00:00+02:00"),
            Categories = ["science-tech"]
        };
    }
}
