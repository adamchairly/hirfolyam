using MediatR;

namespace Hirfolyam.Application.Features;

public sealed class ImportArticlesCommand : IRequest<ImportArticlesResponse>;

public sealed class ImportArticlesResponse
{
    public int Imported { get; set; }

    public int Duplicates { get; set; }

    public int Skipped { get; set; }

    public int FailedFeeds { get; set; }

    public int FailedArticles { get; set; }
}
