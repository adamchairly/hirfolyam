using Hirfolyam.Application.Abstractions;
using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class ArticleImporter(
    IApplicationDbContext context,
    CategoryMapper categoryMapper,
    TimeProvider timeProvider) : IArticleImporter
{
    public async Task<bool> ImportAsync(
        int sourceId,
        string sourceKey,
        RssArticle rssArticle,
        CancellationToken cancellationToken)
    {
        var url = UrlNormalizer.Normalize(rssArticle.Url);
        var externalId = string.IsNullOrWhiteSpace(rssArticle.ExternalId)
            ? url
            : rssArticle.ExternalId.Trim();

        var identity = ArticleIdentityFactory.Create(sourceId, externalId, url);

        if (await ExistsAsync(identity, cancellationToken))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(rssArticle.Title))
        {
            throw new InvalidDataException("The article title is empty.");
        }

        var now = timeProvider.GetUtcNow();
        var article = new Article
        {
            NewsSourceId = sourceId,
            CategoryId = categoryMapper.Map(sourceKey, rssArticle.Categories, url),
            ExternalId = externalId,
            ExternalIdHash = identity.ExternalIdHash!,
            Url = url,
            UrlHash = identity.UrlHash,
            Title = rssArticle.Title,
            Lead = rssArticle.Description,
            LeadSearchText = ArticleSearchText.FromHtml(rssArticle.Description),
            Content = rssArticle.Content,
            Author = rssArticle.Author,
            ImageUrl = rssArticle.ImageUrl,
            PublishedAt = rssArticle.PublishedAt?.ToUniversalTime()
                ?? throw new InvalidDataException("The publication date is missing."),
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
            context.Entry(article).State = EntityState.Detached;

            // Another import stored the same article after the check above.
            if (await ExistsAsync(identity, cancellationToken))
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

    private Task<bool> ExistsAsync(ArticleIdentity identity, CancellationToken cancellationToken)
    {
        return context.Articles.AnyAsync(
            article => article.NewsSourceId == identity.NewsSourceId
                && ((identity.ExternalIdHash != null && article.ExternalIdHash == identity.ExternalIdHash)
                    || article.UrlHash == identity.UrlHash),
            cancellationToken);
    }
}
