using BookNetwork.Application.Common.Repositories.Books;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed class GetBooksForAdminQueryHandler(IBookReadRepository bookReadRepository)
    : IRequestHandler<GetBooksForAdminQueryRequest, GetBooksForAdminQueryResponse>
{
    public async Task<GetBooksForAdminQueryResponse> Handle(
        GetBooksForAdminQueryRequest request,
        CancellationToken cancellationToken)
    {
        var books = await bookReadRepository.Table
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .Select(book => new AdminBookListItemDto(
                book.Id,
                book.Title,
                book.Description,
                book.Isbn,
                book.PublicationYear,
                book.PublisherId))
            .ToListAsync(cancellationToken);

        return new GetBooksForAdminQueryResponse
        {
            Books = books
        };
    }
}
