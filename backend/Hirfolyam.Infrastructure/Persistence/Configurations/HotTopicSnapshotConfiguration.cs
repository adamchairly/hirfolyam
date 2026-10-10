using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class HotTopicSnapshotConfiguration : IEntityTypeConfiguration<HotTopicSnapshot>
{
    public void Configure(EntityTypeBuilder<HotTopicSnapshot> builder)
    {
        builder.HasKey(snapshot => snapshot.Id);
        builder.Property(snapshot => snapshot.Model).HasMaxLength(100);
        builder.Property(snapshot => snapshot.CostUsd).HasPrecision(18, 10);
        builder.HasIndex(snapshot => snapshot.GeneratedAt);
    }
}
