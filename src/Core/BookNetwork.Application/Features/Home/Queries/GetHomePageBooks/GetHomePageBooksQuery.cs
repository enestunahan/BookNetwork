using MediatR;

namespace BookNetwork.Application.Features.Home.Queries.GetHomePageBooks;

public sealed record GetHomePageBooksQuery : IRequest<IReadOnlyList<HomePageBookDto>>;
