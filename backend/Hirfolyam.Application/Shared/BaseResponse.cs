using System.Text.Json.Serialization;

namespace Hirfolyam.Application.Shared;

public class BaseResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("code")]
    public int? Code { get; init; }
}
