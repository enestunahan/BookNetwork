using MediatR;

namespace BookNetwork.Application.Features.Roles.Commands.AssignRoleToUser;

public sealed record AssignRoleToUserCommand(string UserId, string[] RoleNames) : IRequest<Unit>;
