
namespace Hirfolyam.Application.Features;

public interface IRssFeedReader
{
    Task<IReadOnlyCollection<RssArticle>> ReadAsync(string feedUrl, CancellationToken cancellationToken);
}
