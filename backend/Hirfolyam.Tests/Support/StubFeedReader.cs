using Hirfolyam.Application.Features;

namespace Hirfolyam.Tests.Support;

internal sealed class StubFeedReader(Dictionary<string, IReadOnlyCollection<RssArticle>> feeds) : IRssFeedReader
{
    public List<string> Calls { get; } = [];

    public Task<IReadOnlyCollection<RssArticle>> ReadAsync(string feedUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(feedUrl);

        if (!feeds.TryGetValue(feedUrl, out var articles))
        {
            throw new HttpRequestException("The feed is unavailable.");
        }

        return Task.FromResult(articles);
    }

    public Task<string?> FindPageImageAsync(string articleUrl, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }
}
