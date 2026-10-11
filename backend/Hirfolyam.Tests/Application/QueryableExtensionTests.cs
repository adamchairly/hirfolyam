using Hirfolyam.Application.Extensions;
using Hirfolyam.Domain.Entities;
using Hirfolyam.Tests.Support;

namespace Hirfolyam.Tests.Application;

public sealed class QueryableExtensionTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-10T12:00:00Z");

    [TestCase(1, new long[] { 1, 2 })]
    [TestCase(3, new long[] { 5 })]
    [TestCase(4, new long[0])]
    public async Task ThePageHoldsItsItemsAndTheTotals(int page, long[] expected)
    {
        await using var context = TestDatabase.Create(Guid.NewGuid().ToString());
        context.Articles.AddRange(Enumerable.Range(1, 5).Select(id => CreateArticle(id)));
        await context.SaveChangesAsync();

        var result = await context.Articles
            .OrderBy(article => article.Id)
            .Select(article => article.Id)
            .ToPagedResultAsync(page, 2, CancellationToken.None);

        Assert.That(result.Items, Is.EqualTo(expected));
        Assert.That(result.Page, Is.EqualTo(page));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalItems, Is.EqualTo(5));
        Assert.That(result.TotalPages, Is.EqualTo(3));
    }

    private static Article CreateArticle(long id)
    {
        return new Article
        {
            Id = id,
            NewsSourceId = 1,
            ExternalId = id.ToString(),
            ExternalIdHash = id.ToString(),
            Url = $"https://example.com/{id}",
            UrlHash = id.ToString(),
            Title = $"Title {id}",
            PublishedAt = Now,
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }
}
