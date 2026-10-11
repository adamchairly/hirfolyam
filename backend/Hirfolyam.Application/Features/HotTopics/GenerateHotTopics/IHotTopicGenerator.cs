namespace Hirfolyam.Application.Features;

public interface IHotTopicGenerator
{
    string Model { get; }

    Task<HotTopicGeneration> GenerateAsync(
        IReadOnlyList<HotTopicArticle> articles,
        int topicCount,
        CancellationToken cancellationToken);
}
