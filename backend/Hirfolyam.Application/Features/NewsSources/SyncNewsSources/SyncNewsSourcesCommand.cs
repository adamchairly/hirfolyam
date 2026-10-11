using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class SyncNewsSourcesCommand : IRequest<SyncNewsSourcesResult>;

public sealed record SyncNewsSourcesResult(int Added, int Updated, int Disabled);
