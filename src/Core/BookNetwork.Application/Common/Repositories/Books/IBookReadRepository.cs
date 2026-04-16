using BookNetwork.Application.Features.Home.Queries.GetHomePageBooks;
using BookNetwork.Domain.Entities;

namespace BookNetwork.Application.Common.Repositories.Books;

public interface IBookReadRepository : IReadRepository<Book>
{
    Task<IReadOnlyList<HomePageBookDto>> GetHomePageBooksAsync(CancellationToken cancellationToken = default);
}
