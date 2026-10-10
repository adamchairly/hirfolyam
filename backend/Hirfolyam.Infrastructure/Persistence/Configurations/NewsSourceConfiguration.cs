using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class NewsSourceConfiguration : IEntityTypeConfiguration<NewsSource>
{
    public void Configure(EntityTypeBuilder<NewsSource> builder)
    {
        builder.HasKey(source => source.Id);
        builder.Property(source => source.Key).HasMaxLength(100);
        builder.Property(source => source.Name).HasMaxLength(200);
        builder.HasIndex(source => source.Key).IsUnique();
    }
}
