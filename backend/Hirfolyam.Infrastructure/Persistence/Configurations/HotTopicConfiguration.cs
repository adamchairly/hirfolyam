using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class HotTopicConfiguration : IEntityTypeConfiguration<HotTopic>
{
    public void Configure(EntityTypeBuilder<HotTopic> builder)
    {
        builder.ToTable("HotTopics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Title).HasMaxLength(HotTopic.TitleMaxLength);
        builder.Property(topic => topic.Summary).HasMaxLength(HotTopic.SummaryMaxLength);

        builder.HasOne(topic => topic.Snapshot)
            .WithMany(snapshot => snapshot.Topics)
            .HasForeignKey(topic => topic.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(topic => new
        {
            topic.SnapshotId,
            topic.Rank
        })
            .IsUnique();
    }
}
