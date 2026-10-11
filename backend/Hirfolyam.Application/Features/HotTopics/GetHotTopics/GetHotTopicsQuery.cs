using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GetHotTopicsQuery : IRequest<GetHotTopicsResponse>;

public sealed class GetHotTopicsResponse
{
    public required IReadOnlyList<HotTopicResponse> Items { get; init; }

    public DateTimeOffset? GeneratedAt { get; init; }
}

public sealed class HotTopicResponse
{
    public long Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<HotTopicArticleResponse> Articles { get; init; }
}

public sealed class HotTopicArticleResponse
{
    public long Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public DateTimeOffset PublishedAt { get; init; }

    public required ArticleSourceResponse Source { get; init; }
}
