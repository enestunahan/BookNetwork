using MediatR;

namespace BookNetwork.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string UserId) : IRequest<Unit>;
