using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GetSourcesQuery : IRequest<GetSourcesResponse>;

public sealed class GetSourcesResponse
{
    public required IReadOnlyList<SourceListItemResponse> Items { get; init; }

    public DateTimeOffset? LastUpdatedAt { get; init; }
}

public sealed class SourceListItemResponse
{
    public required string Key { get; init; }

    public required string Name { get; init; }

    public required string Url { get; init; }

    public string? IconUrl { get; init; }

    // The slug of the category the source belongs to.
    public required string Category { get; init; }

    public int ArticleCount { get; init; }
}
