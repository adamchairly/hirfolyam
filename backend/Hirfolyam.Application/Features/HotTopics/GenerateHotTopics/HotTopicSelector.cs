using Hirfolyam.Domain.Entities;

namespace Hirfolyam.Application.Features;

// The model groups and summarizes; the checks and the order are decided here.
internal static class HotTopicSelector
{
    public static List<HotTopic> Select(
        IReadOnlyList<HotTopicDraft> drafts,
        Dictionary<long, int> sourceIdsByArticleId,
        int topicCount)
    {
        var usedIds = new HashSet<long>();
        var candidates = new List<(string Title, string Summary, List<long> ArticleIds, int Sources)>();

        foreach (var draft in drafts)
        {
            // Unknown ids are dropped, and an article counts only for the first kept topic that claims it.
            var ids = draft.ArticleIds
                .Where(id => sourceIdsByArticleId.ContainsKey(id) && !usedIds.Contains(id))
                .Distinct()
                .ToList();
            var title = draft.Title.Trim();
            var summary = draft.Summary.Trim();
            var sources = ids.Select(id => sourceIdsByArticleId[id]).Distinct().Count();

            if (sources < HotTopic.MinSources
                || title.Length is 0 or > HotTopic.TitleMaxLength
                || summary.Length is 0 or > HotTopic.SummaryMaxLength)
            {
                continue;
            }

            usedIds.UnionWith(ids);
            candidates.Add((title, summary, ids, sources));
        }

        // The sort is stable, so equal topics keep the order of the model.
        return candidates
            .OrderByDescending(candidate => candidate.Sources)
            .ThenByDescending(candidate => candidate.ArticleIds.Count)
            .Take(topicCount)
            .Select((candidate, index) => new HotTopic
            {
                Rank = index + 1,
                Title = candidate.Title,
                Summary = candidate.Summary,
                ArticleIds = [.. candidate.ArticleIds]
            })
            .ToList();
    }
}
