using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hirfolyam.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Article> Articles { get; }

    DbSet<NewsSource> NewsSources { get; }

    DbSet<HotTopicSnapshot> HotTopicSnapshots { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
