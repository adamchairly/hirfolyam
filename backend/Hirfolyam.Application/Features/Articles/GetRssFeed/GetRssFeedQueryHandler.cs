using System.Globalization;
using System.Xml.Linq;
using Hirfolyam.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class GetRssFeedQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetRssFeedQuery, string>
{
    private const int ItemCount = 50;

    private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace Media = "http://search.yahoo.com/mrss/";

    public async Task<string> Handle(GetRssFeedQuery request, CancellationToken cancellationToken)
    {
        var articles = await context.Articles
            .AsNoTracking()
            .OrderByDescending(article => article.PublishedAt)
            .ThenByDescending(article => article.Id)
            .Take(ItemCount)
            .Select(article => new
            {
                article.Title,
                article.Url,
                article.LeadText,
                article.Author,
                article.ImageUrl,
                article.PublishedAt,
                SourceName = article.NewsSource.Name,
                SourceUrl = article.NewsSource.BaseUrl,
                CategoryName = article.Category == null ? null : article.Category.Name
            })
            .ToListAsync(cancellationToken);

        var channel = new XElement("channel",
            new XElement("title", "hírfolyam"),
            new XElement("link", request.SiteUrl),
            new XElement("description", "Híraggregátor több mint 60 portáltól"),
            new XElement("language", "hu"),
            articles.Count == 0 ? null : new XElement("lastBuildDate", FormatDate(articles[0].PublishedAt)),
            articles.Select(article => new XElement("item",
                new XElement("title", article.Title),
                new XElement("link", article.Url),
                new XElement("guid", article.Url),
                new XElement("pubDate", FormatDate(article.PublishedAt)),
                string.IsNullOrEmpty(article.LeadText) ? null : new XElement("description", article.LeadText),
                article.Author is null ? null : new XElement(Dc + "creator", article.Author),
                article.CategoryName is null ? null : new XElement("category", article.CategoryName),
                new XElement("source", new XAttribute("url", article.SourceUrl), article.SourceName),
                article.ImageUrl is null
                    ? null
                    : new XElement(Media + "content",
                        new XAttribute("url", article.ImageUrl),
                        new XAttribute("medium", "image")))));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "dc", Dc),
                new XAttribute(XNamespace.Xmlns + "media", Media),
                channel));

        return document.Declaration + Environment.NewLine + document;
    }

    // RFC 822 date, as RSS 2.0 requires.
    private static string FormatDate(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);
    }
}
