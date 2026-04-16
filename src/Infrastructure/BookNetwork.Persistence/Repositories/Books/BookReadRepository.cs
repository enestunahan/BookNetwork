using BookNetwork.Application.Common.Repositories.Books;
using BookNetwork.Application.Features.Home.Queries.GetHomePageBooks;
using BookNetwork.Domain.Entities;
using BookNetwork.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Persistence.Repositories.Books;

public sealed class BookReadRepository(BookNetworkDbContext context)
    : ReadRepository<Book>(context), IBookReadRepository
{
    public async Task<IReadOnlyList<HomePageBookDto>> GetHomePageBooksAsync(CancellationToken cancellationToken = default)
    {
        return await Table
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .Select(book => new HomePageBookDto(
                book.Id,
                book.Title,
                book.Publisher!.Name,
                book.BookAuthors
                    .OrderBy(bookAuthor => bookAuthor.Author!.LastName)
                    .ThenBy(bookAuthor => bookAuthor.Author!.FirstName)
                    .Select(bookAuthor => new HomePageBookAuthorDto(
                        bookAuthor.Author!.FirstName,
                        bookAuthor.Author!.LastName))
                    .ToList(),
                book.BookCategories
                    .OrderBy(bookCategory => bookCategory.Category!.Name)
                    .Select(bookCategory => bookCategory.Category!.Name)
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
