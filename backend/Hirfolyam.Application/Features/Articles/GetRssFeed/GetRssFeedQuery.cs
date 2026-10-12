using MediatR;

namespace Hirfolyam.Application.Features;

// Returns the RSS 2.0 document of the newest articles.
public sealed class GetRssFeedQuery : IRequest<string>
{
    public required string SiteUrl { get; init; }
}
