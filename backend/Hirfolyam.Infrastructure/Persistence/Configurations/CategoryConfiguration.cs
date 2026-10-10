using Hirfolyam.Domain.Catalogs;
using Hirfolyam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hirfolyam.Infrastructure.Persistence;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(100);
        builder.Property(category => category.Slug).HasMaxLength(64);
        builder.HasIndex(category => category.Slug).IsUnique();
        builder.HasData(Categories.All);
    }
}
