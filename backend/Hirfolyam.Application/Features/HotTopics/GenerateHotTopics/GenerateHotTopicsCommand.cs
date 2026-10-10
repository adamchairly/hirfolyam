using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class GenerateHotTopicsCommand : IRequest<GenerateHotTopicsResponse>;

public sealed class GenerateHotTopicsResponse
{
    public bool Succeeded { get; set; }

    public int Topics { get; set; }
}
