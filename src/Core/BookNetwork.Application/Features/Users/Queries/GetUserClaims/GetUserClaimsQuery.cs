using MediatR;

namespace BookNetwork.Application.Features.Users.Queries.GetUserClaims;

public sealed record GetUserClaimsQuery(string UserId) : IRequest<IReadOnlyList<ClaimDto>>;

public sealed record ClaimDto(string Type, string Value);
