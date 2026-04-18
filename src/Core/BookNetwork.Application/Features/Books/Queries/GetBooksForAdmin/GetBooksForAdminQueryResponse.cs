namespace BookNetwork.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed record AdminBookListItemDto(
    Guid Id,
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    Guid PublisherId);

public sealed class GetBooksForAdminQueryResponse
{
    public IReadOnlyList<AdminBookListItemDto> Books { get; init; } = [];
}
