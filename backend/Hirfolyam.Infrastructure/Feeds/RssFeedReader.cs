using System.Globalization;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using Hirfolyam.Application.Features;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Sagara.FeedReader.Feeds;
using Sagara.FeedReader;

namespace Hirfolyam.Infrastructure.Feeds;

public sealed class RssFeedReader(HttpClient client, ILogger<RssFeedReader> logger) : IRssFeedReader
{
    public const int MaximumResponseBytes = 5 * 1024 * 1024;

    public async Task<IReadOnlyCollection<RssArticle>> ReadAsync(string feedUrl, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(feedUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Relative links are resolved against the address after redirects.
        var feedUri = response.RequestMessage?.RequestUri ?? new Uri(feedUrl);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaximumResponseBytes,
            Async = true
        });

        var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
        NormalizeDates(document);

        var feed = FeedReader.ReadFromString(document.ToString(SaveOptions.DisableFormatting));
        var articles = new List<RssArticle>();

        foreach (var item in feed.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                articles.Add(MapArticle(item, feedUri));
            }
            catch (Exception exception) when (
                exception is XmlException or FormatException or ArgumentException or InvalidDataException)
            {
                // The same broken item comes back on every run, so it is not logged as a warning.
                logger.LogDebug(exception, "Cannot read a feed item in {FeedUrl}: {ArticleUrl}", feedUrl, item.Link);
            }
        }

        return articles;
    }

    private static RssArticle MapArticle(FeedItem item, Uri feedUri)
    {
        var element = item.SpecificItem?.ItemOrEntryElement;
        var atom = item.SpecificItem as AtomFeedItem;
        var link = atom is null
            ? item.Link
            : atom.Links.FirstOrDefault(link => link.Relation is null or "alternate")?.Href;
        var url = AbsoluteUrl(link, feedUri)
            ?? throw new InvalidDataException("The feed item has no valid article URL.");
        var title = WebUtility.HtmlDecode(item.Title);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidDataException("The feed item has no title.");
        }

        var articleUri = new Uri(url);
        var publishedAt = (item.PublishingDate ?? item.SpecificItem switch
        {
            AtomFeedItem { PublishedDateString: null } entry => entry.UpdatedDate,
            Rss20FeedItem { PublishingDateString: null } rss => rss.DC?.Date,
            MediaRssFeedItem { PublishingDateString: null } rss => rss.DC?.Date,
            _ => null
        }) ?? throw new InvalidDataException("The feed item has no publication date.");

        var author = atom is null
            ? string.IsNullOrWhiteSpace(item.Author) ? Value(element, "creator") : item.Author
            : string.Join(", ", element!.Elements(element.Name.Namespace + "author")
                .Select(author => new AtomPerson(author))
                .Select(author => author.Name ?? author.EMail)
                .Where(name => !string.IsNullOrWhiteSpace(name)));

        // Preserve the content supplied by the feed.
        var description = atom is null ? Value(element, "description") : Value(element, "summary");
        var content = atom is null
            ? Value(element, "encoded")
            : Value(element, "content");

        return new RssArticle
        {
            ExternalId = string.IsNullOrWhiteSpace(item.Id) ? null : item.Id,
            Title = title,
            Url = url,
            PublishedAt = new DateTimeOffset(publishedAt).ToUniversalTime(),
            Author = string.IsNullOrWhiteSpace(author) ? null : WebUtility.HtmlDecode(author),
            Description = description,
            Content = content,
            Categories = item.Categories.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray(),
            ImageUrl = FindImage(element, articleUri)
                ?? FindHtmlImage(description, articleUri)
                ?? FindHtmlImage(content, articleUri)
        };
    }

    private static string? FindImage(XElement? element, Uri articleUri)
    {
        foreach (var child in element?.Descendants() ?? [])
        {
            var type = child.Attribute("type")?.Value;
            var medium = child.Attribute("medium")?.Value;
            var media = child.Name.NamespaceName.Contains("search.yahoo.com/mrss", StringComparison.Ordinal);
            var isImage = child.Name.LocalName == "thumbnail"
                || type?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true
                || medium == "image"
                || (media && child.Name.LocalName == "content" && type is null && medium is null);

            if (isImage)
            {
                var url = AbsoluteUrl(child.Attribute("url")?.Value ?? child.Attribute("href")?.Value, articleUri);

                if (url is not null)
                {
                    return url;
                }
            }
        }

        return null;
    }

    // Some feeds only embed the image in the HTML of the item.
    private static string? FindHtmlImage(string? html, Uri articleUri)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);

        return document.DocumentNode
            .Descendants("img")
            .Select(image => AbsoluteUrl(WebUtility.HtmlDecode(image.GetAttributeValue("src", null)), articleUri))
            .FirstOrDefault(url => url is not null);
    }

    private static string? Value(XElement? element, string name)
    {
        var child = element?.Elements().FirstOrDefault(child => child.Name.LocalName == name);

        return child?.HasElements == true
            ? string.Concat(child.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)))
            : child?.Value;
    }

    private static string? AbsoluteUrl(string? value, Uri baseUrl)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(baseUrl, value.Trim(), out var url)
            || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(url.UserInfo))
        {
            return null;
        }

        return url.AbsoluteUri;
    }

    private static void NormalizeDates(XDocument document)
    {
        // The parser can throw for invalid dates.
        var dates = document.Descendants().Where(element =>
            element.Parent?.Name.LocalName is "channel" or "feed" or "item" or "entry"
            && element.Name.LocalName is "pubDate" or "published" or "updated" or "date" or "lastBuildDate");

        foreach (var element in dates)
        {
            var valid = DateTimeOffset.TryParse(
                element.Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out var date);

            element.Value = valid ? date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}
