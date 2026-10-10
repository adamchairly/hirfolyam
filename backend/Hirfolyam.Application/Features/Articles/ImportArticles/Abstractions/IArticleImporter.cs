namespace Hirfolyam.Application.Features;

public interface IArticleImporter
{
    Task<bool> ImportAsync(
        int sourceId,
        string sourceKey,
        RssArticle rssArticle,
        CancellationToken cancellationToken);
}
