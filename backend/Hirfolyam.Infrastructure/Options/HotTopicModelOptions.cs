namespace Hirfolyam.Infrastructure.Options;

public sealed class HotTopicModelOptions
{
    public string Model { get; set; } = "openai/gpt-6-luna";

    // Sent only to reasoning models; leave empty for models without the reasoning parameter.
    public string? ReasoningEffort { get; set; } = "low";

    // Reasoning tokens count towards the limit too.
    public int MaxTokens { get; set; } = 16000;

    public int TimeoutSeconds { get; set; } = 120;
}
