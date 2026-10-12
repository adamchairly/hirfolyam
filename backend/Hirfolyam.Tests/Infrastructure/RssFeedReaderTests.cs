using System.Net;
using System.Text;
using Hirfolyam.Application.Features;
using Hirfolyam.Infrastructure.Feeds;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hirfolyam.Tests.Infrastructure;

public sealed class RssFeedReaderTests
{
    private const string FeedUrl = "https://example.com/feed";

    [Test]
    public async Task ItemsWithoutTitleLinkOrPublicationDateAreLeftOut()
    {
        var articles = await ReadAsync("""
            <rss version="2.0">
              <channel>
                <title>Example</title>
                <link>https://example.com/</link>
                <item>
                  <title>Complete</title>
                  <link>https://example.com/complete</link>
                  <pubDate>Tue, 06 Oct 2026 12:00:00 +0200</pubDate>
                </item>
                <item>
                  <title> </title>
                  <link>https://example.com/no-title</link>
                  <pubDate>Tue, 06 Oct 2026 12:00:00 +0200</pubDate>
                </item>
                <item>
                  <title>No link</title>
                  <pubDate>Tue, 06 Oct 2026 12:00:00 +0200</pubDate>
                </item>
                <item>
                  <title>No date</title>
                  <link>https://example.com/no-date</link>
                </item>
              </channel>
            </rss>
            """);

        var article = articles.Single();

        Assert.That(article.Title, Is.EqualTo("Complete"));
        Assert.That(article.Url, Is.EqualTo("https://example.com/complete"));
        Assert.That(article.PublishedAt, Is.EqualTo(DateTimeOffset.Parse("2026-10-06T10:00:00Z")));
        Assert.That(article.PublishedAt.Offset, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public async Task ImageIsTakenFromItemHtmlWhenFeedHasNoMediaElement()
    {
        var articles = await ReadAsync("""
            <rss version="2.0" xmlns:content="http://purl.org/rss/1.0/modules/content/">
              <channel>
                <title>Example</title>
                <link>https://example.com/</link>
                <item>
                  <title>Lead image</title>
                  <link>https://example.com/lead</link>
                  <pubDate>Tue, 06 Oct 2026 12:00:00 +0200</pubDate>
                  <description><![CDATA[<img src="/images/lead.jpg?a=1&amp;b=2" />Lead]]></description>
                  <content:encoded><![CDATA[<p><img src="https://example.com/content.jpg" /></p>]]></content:encoded>
                </item>
                <item>
                  <title>Content image</title>
                  <link>https://example.com/content</link>
                  <pubDate>Tue, 06 Oct 2026 12:00:00 +0200</pubDate>
                  <description>Lead</description>
                  <content:encoded><![CDATA[<p><img src="data:image/gif;base64,R0lGOD" /><img src="https://example.com/content.jpg" /></p>]]></content:encoded>
                </item>
              </channel>
            </rss>
            """);

        Assert.That(
            articles.Select(article => article.ImageUrl),
            Is.EqualTo(new[] { "https://example.com/images/lead.jpg?a=1&b=2", "https://example.com/content.jpg" }));
    }

    [Test]
    public async Task PageImageIsTakenFromOpenGraphMetadata()
    {
        using var client = new HttpClient(new StubHandler("""
            <html><head><meta property="og:image" content="/images/cover.jpg?a=1&amp;b=2"></head></html>
            """));
        var reader = new RssFeedReader(client, NullLogger<RssFeedReader>.Instance);

        var image = await reader.FindPageImageAsync("https://example.com/article", CancellationToken.None);

        Assert.That(image, Is.EqualTo("https://example.com/images/cover.jpg?a=1&b=2"));
    }

    private static async Task<IReadOnlyCollection<RssArticle>> ReadAsync(string feed)
    {
        using var client = new HttpClient(new StubHandler(feed));
        var reader = new RssFeedReader(client, NullLogger<RssFeedReader>.Instance);

        return await reader.ReadAsync(FeedUrl, CancellationToken.None);
    }

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/rss+xml"),
                RequestMessage = request
            });
        }
    }
}
