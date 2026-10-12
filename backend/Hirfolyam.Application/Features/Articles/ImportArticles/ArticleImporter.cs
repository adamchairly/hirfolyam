using System.Security.Cryptography;
using System.Text;
using Hirfolyam.Application.Abstractions;
using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class ArticleImporter(
    IApplicationDbContext context,
    CategoryMapper categoryMapper,
    IRssFeedReader feedReader,
    TimeProvider timeProvider)
{
    // Returns false when the article is already stored.
    public async Task<bool> ImportAsync(NewsSource source, RssArticle rssArticle, CancellationToken cancellationToken)
    {
        var url = UrlNormalizer.Normalize(rssArticle.Url);
        var externalId = string.IsNullOrWhiteSpace(rssArticle.ExternalId)
            ? url
            : rssArticle.ExternalId.Trim();
        var externalIdHash = Hash(externalId);
        var urlHash = Hash(url);

        // Do not import already existing articles
        if (await ExistsAsync(source.Id, externalIdHash, urlHash, cancellationToken))
        {
            return false;
        }

        var now = timeProvider.GetUtcNow();
        var article = new Article
        {
            NewsSourceId = source.Id,
            CategoryId = categoryMapper.Map(source.Key, rssArticle.Categories, url),
            ExternalId = externalId,
            ExternalIdHash = externalIdHash,
            Url = url,
            UrlHash = urlHash,
            Title = rssArticle.Title,
            Lead = rssArticle.Description,
            LeadText = PlainText.FromHtml(rssArticle.Description),
            Content = rssArticle.Content,
            Author = rssArticle.Author,
            // Some feeds have no images, at this point we know this is a new article
            ImageUrl = rssArticle.ImageUrl ?? await feedReader.FindPageImageAsync(url, cancellationToken),
            PublishedAt = rssArticle.PublishedAt.ToUniversalTime(),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Articles.Add(article);

        try
        {
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException)
        {
            // Another import stored the same article after the check above.
            if (await ExistsAsync(source.Id, externalIdHash, urlHash, cancellationToken))
            {
                return false;
            }

            throw;
        }
        finally
        {
            // A run touches thousands of articles; keep the change tracker small.
            context.Entry(article).State = EntityState.Detached;
        }
    }

    internal static string Hash(string value)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private Task<bool> ExistsAsync(int sourceId, string externalIdHash, string urlHash, CancellationToken cancellationToken)
    {
        return context.Articles.AnyAsync(
            article => article.NewsSourceId == sourceId
                && (article.ExternalIdHash == externalIdHash || article.UrlHash == urlHash),
            cancellationToken);
    }
}
