using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GetVersionQuery : IRequest<GetVersionResponse>;

public sealed class GetVersionResponse
{
    // Coolify sets SOURCE_COMMIT at runtime, it is missing in local runs.
    public string? Commit { get; init; }
}
