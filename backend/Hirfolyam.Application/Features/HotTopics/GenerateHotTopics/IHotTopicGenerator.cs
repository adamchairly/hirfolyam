namespace Hirfolyam.Application.Features;

public interface IHotTopicGenerator
{
    string Model { get; }

    Task<HotTopicGeneration> GenerateAsync(
        IReadOnlyList<HotTopicArticle> articles,
        int topicCount,
        CancellationToken cancellationToken);

    // Merges the hourly topics of one day into the stories of the day.
    Task<DailyTopicGeneration> GenerateDailyAsync(
        IReadOnlyList<DailyTopicInput> topics,
        int topicCount,
        CancellationToken cancellationToken);
}
