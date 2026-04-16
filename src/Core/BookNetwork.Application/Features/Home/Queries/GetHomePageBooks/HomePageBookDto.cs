namespace BookNetwork.Application.Features.Home.Queries.GetHomePageBooks;

public sealed record HomePageBookDto(
    Guid Id,
    string Title,
    string PublisherName,
    List<HomePageBookAuthorDto> Authors,
    List<string> Categories);

public sealed record HomePageBookAuthorDto(
    string FirstName,
    string LastName);
