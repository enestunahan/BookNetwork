using BookNetwork.Application.Common.Repositories.Books;
using BookNetwork.Domain.Entities;
using BookNetwork.Persistence.Contexts;

namespace BookNetwork.Persistence.Repositories.Books;

public sealed class BookWriteRepository(BookNetworkDbContext context)
    : WriteRepository<Book>(context), IBookWriteRepository;
