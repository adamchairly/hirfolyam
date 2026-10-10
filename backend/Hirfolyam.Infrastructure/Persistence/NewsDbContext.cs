using Hirfolyam.Application.Abstractions;
using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class NewsDbContext(DbContextOptions<NewsDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Article> Articles => Set<Article>();

    public DbSet<NewsSource> NewsSources => Set<NewsSource>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<HotTopicSnapshot> HotTopicSnapshots => Set<HotTopicSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsDbContext).Assembly);
    }
}
