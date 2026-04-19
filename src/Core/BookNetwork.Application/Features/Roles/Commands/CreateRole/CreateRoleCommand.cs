using MediatR;

namespace BookNetwork.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string Name) : IRequest<CreateRoleResponse>;

public sealed record CreateRoleResponse(string Id, string Name);
