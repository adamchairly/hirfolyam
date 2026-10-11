namespace Hirfolyam.Application.Features;

public sealed record HotTopicArticle(long Id, int SourceId, string SourceName, string Title, string LeadText);

public sealed record HotTopicDraft(string Title, string Summary, IReadOnlyList<long> ArticleIds);

public sealed record HotTopicGeneration(
    IReadOnlyList<HotTopicDraft> Topics,
    int? InputTokens = null,
    int? OutputTokens = null,
    decimal? CostUsd = null);
