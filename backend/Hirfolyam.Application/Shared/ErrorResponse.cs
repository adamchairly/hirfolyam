namespace Hirfolyam.Application.Shared;

public sealed class ErrorResponse
{
    public string? Error { get; init; }

    public string? Message { get; init; }

    public int? Code { get; init; }
}
