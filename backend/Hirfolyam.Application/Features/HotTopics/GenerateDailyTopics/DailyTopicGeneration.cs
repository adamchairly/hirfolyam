namespace Hirfolyam.Application.Features;

// An hourly topic of the day.
public sealed record DailyTopicInput(long Id, string Title, string Summary);

// A story of the day, made of the hourly topics with these ids.
public sealed record DailyTopicDraft(string Title, string Summary, IReadOnlyList<long> TopicIds);

public sealed record DailyTopicGeneration(
    IReadOnlyList<DailyTopicDraft> Topics,
    int? InputTokens = null,
    int? OutputTokens = null,
    decimal? CostUsd = null);
