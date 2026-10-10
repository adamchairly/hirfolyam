using Hirfolyam.Domain.Entities;

namespace Hirfolyam.Domain.Catalogs;

public static class Categories
{
    // Desired categories of the site, with corresponding slugs
    public static IReadOnlyList<Category> All { get; } =
    [
        new()
        {
            Id = 1,
            Name = "Politika",
            Slug = "politics"
        },
        new()
        {
            Id = 2,
            Name = "Gazdaság",
            Slug = "economy"
        },
        new()
        {
            Id = 3,
            Name = "Külföld",
            Slug = "world"
        },
        new()
        {
            Id = 4,
            Name = "Belföld",
            Slug = "domestic"
        },
        new()
        {
            Id = 5,
            Name = "Kultúra",
            Slug = "culture"
        },
        new()
        {
            Id = 6,
            Name = "Tudomány",
            Slug = "science-tech"
        },
        new()
        {
            Id = 7,
            Name = "Sport",
            Slug = "sport"
        },
        new()
        {
            Id = 8,
            Name = "Bulvár",
            Slug = "tabloid"
        },
        new()
        {
            Id = 9,
            Name = "Életmód",
            Slug = "lifestyle"
        }
    ];
}
