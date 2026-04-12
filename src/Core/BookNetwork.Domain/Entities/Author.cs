using BookNetwork.Domain.Common;

namespace BookNetwork.Domain.Entities;

public sealed class Author : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateTime? BirthDate { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
