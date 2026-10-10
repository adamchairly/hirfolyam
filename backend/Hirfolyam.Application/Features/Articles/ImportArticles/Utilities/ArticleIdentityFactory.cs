
namespace Hirfolyam.Application.Features;

public static class ArticleIdentityFactory
{
    public static ArticleIdentity Create(int sourceId, string? externalId, string url)
    {
        var externalIdHash = externalId is null ? null : UrlNormalizer.Hash(externalId);
        var urlHash = UrlNormalizer.Hash(UrlNormalizer.Normalize(url));

        return new ArticleIdentity(sourceId, externalIdHash, urlHash);
    }
}
