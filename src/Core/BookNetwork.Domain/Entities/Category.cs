using BookNetwork.Domain.Common;

namespace BookNetwork.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
