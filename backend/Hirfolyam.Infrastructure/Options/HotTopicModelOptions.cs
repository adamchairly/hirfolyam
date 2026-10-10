namespace Hirfolyam.Infrastructure.Options;

public sealed class HotTopicModelOptions
{
    public const string SectionName = "OpenRouter:HotTopics";

    public string Model { get; set; } = "openai/gpt-6-luna";

    // Sent only to reasoning models; leave empty for models without the reasoning parameter.
    public string? ReasoningEffort { get; set; } = "low";

    public int TimeoutSeconds { get; set; } = 120;
}
