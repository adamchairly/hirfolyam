namespace Hirfolyam.Application.Features;

public sealed record ArticleIdentity(
    int NewsSourceId,
    string? ExternalIdHash,
    string UrlHash);
