using Hirfolyam.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Application.Features;

internal sealed class GetSourcesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSourcesQuery, GetSourcesResponse>
{
    public async Task<GetSourcesResponse> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        var sources = await context.NewsSources
            .AsNoTracking()
            .OrderBy(source => source.Name)
            .ThenBy(source => source.Key)
            .Select(source => new SourceListItemResponse
            {
                Key = source.Key,
                Name = source.Name,
                Url = source.BaseUrl,
                IconUrl = source.IconUrl,
                Category = source.Category.Slug,
                ArticleCount = context.Articles.Count(article => article.NewsSourceId == source.Id)
            })
            .ToListAsync(cancellationToken);

        var lastUpdatedAt = await context.NewsSources
            .MaxAsync(source => source.LastImportedAt, cancellationToken);

        return new GetSourcesResponse
        {
            Items = sources,
            LastUpdatedAt = lastUpdatedAt
        };
    }
}
