using MediatR;

namespace BookNetwork.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;

public sealed record UserListItemDto(
    string Id,
    string UserName,
    string Email,
    string NameSurname);
