using MediatR;

namespace BookNetwork.Application.Features.Users.Commands.AssignClaim;

public sealed record AssignClaimToUserCommand(string UserId, string ClaimType, string ClaimValue) : IRequest<Unit>;
