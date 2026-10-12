using MediatR;
using Microsoft.Extensions.Configuration;

namespace Hirfolyam.Application.Features;

internal sealed class GetVersionQueryHandler(IConfiguration configuration)
    : IRequestHandler<GetVersionQuery, GetVersionResponse>
{
    public Task<GetVersionResponse> Handle(GetVersionQuery request, CancellationToken cancellationToken)
    {
        var commit = configuration["SOURCE_COMMIT"];

        return Task.FromResult(new GetVersionResponse
        {
            Commit = string.IsNullOrWhiteSpace(commit) ? null : commit.Trim()
        });
    }
}
