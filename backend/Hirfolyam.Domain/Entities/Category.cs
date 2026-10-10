namespace Hirfolyam.Domain.Entities;

public sealed class Category
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }
}
