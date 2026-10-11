using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GetHotTopicQuery : IRequest<GetHotTopicResponse>
{
    public long Id { get; init; }
}

public sealed class GetHotTopicResponse
{
    public long Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public required IReadOnlyList<ArticleListItemResponse> Articles { get; init; }
}
