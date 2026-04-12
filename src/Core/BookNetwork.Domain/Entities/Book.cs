using BookNetwork.Domain.Common;

namespace BookNetwork.Domain.Entities;

public sealed class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int PublicationYear { get; set; }

    public Guid PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
