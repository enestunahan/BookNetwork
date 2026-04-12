using BookNetwork.Domain.Common;

namespace BookNetwork.Domain.Entities;

public sealed class Publisher : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Website { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
