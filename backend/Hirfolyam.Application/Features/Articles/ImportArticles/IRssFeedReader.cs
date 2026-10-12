namespace Hirfolyam.Application.Features;

public interface IRssFeedReader
{
    Task<IReadOnlyCollection<RssArticle>> ReadAsync(string feedUrl, CancellationToken cancellationToken);

    // Returns the og:image of the article page
    Task<string?> FindPageImageAsync(string articleUrl, CancellationToken cancellationToken);
}
