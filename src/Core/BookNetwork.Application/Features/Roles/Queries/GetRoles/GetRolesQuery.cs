using MediatR;

namespace BookNetwork.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public sealed record RoleDto(string Id, string Name);
