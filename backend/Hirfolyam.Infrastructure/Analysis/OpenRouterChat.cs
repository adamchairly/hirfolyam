using System.Text.Json;

namespace Hirfolyam.Infrastructure.Analysis;

// Shared parts of the OpenRouter chat completion calls.
internal static class OpenRouterChat
{
    public const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";

    // Returns the text of the single complete answer.
    public static string ReadContent(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object
            || root.TryGetProperty("error", out _)
            || !root.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() != 1)
        {
            throw new InvalidDataException("The OpenRouter response has no single result.");
        }

        var choice = choices[0];

        if (choice.ValueKind != JsonValueKind.Object
            || !choice.TryGetProperty("finish_reason", out var finishReason)
            || finishReason.ValueKind != JsonValueKind.String
            || finishReason.GetString() != "stop"
            || !choice.TryGetProperty("message", out var message)
            || message.ValueKind != JsonValueKind.Object
            || !message.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException("The OpenRouter response is incomplete.");
        }

        return content.GetString()!;
    }

    public static string LimitText(string value, int limit)
    {
        if (value.Length <= limit)
        {
            return value;
        }

        if (char.IsHighSurrogate(value[limit - 1]))
        {
            limit--;
        }

        return value[..limit];
    }

    public static int? ReadTokenCount(JsonElement root, string name)
    {
        if (root.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var count))
        {
            return count;
        }

        return null;
    }

    public static decimal? ReadCost(JsonElement root)
    {
        if (root.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("cost", out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetDecimal(out var cost))
        {
            return cost;
        }

        return null;
    }
}
