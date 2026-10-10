using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(article => article.Id);
        builder.Property(article => article.ExternalIdHash).HasMaxLength(64);
        builder.Property(article => article.UrlHash).HasMaxLength(64);

        builder.HasOne(article => article.NewsSource)
            .WithMany()
            .HasForeignKey(article => article.NewsSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(article => article.Category)
            .WithMany()
            .HasForeignKey(article => article.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(article => new
        {
            article.NewsSourceId,
            article.ExternalIdHash
        })
            .IsUnique()
            .HasDatabaseName("UX_Articles_Source_ExternalId");

        builder.HasIndex(article => new
        {
            article.NewsSourceId,
            article.UrlHash
        })
            .IsUnique()
            .HasDatabaseName("UX_Articles_Source_Url");

        builder.HasIndex(article => new
        {
            article.PublishedAt,
            article.Id
        })
            .IsDescending();

        builder.HasIndex(article => new
        {
            article.NewsSourceId,
            article.PublishedAt
        });

        builder.HasIndex(article => new
        {
            article.CategoryId,
            article.PublishedAt
        });
    }
}
