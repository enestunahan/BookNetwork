using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Users.Queries.GetUserClaims;

public sealed class GetUserClaimsQueryHandler(UserManager<AppUser> userManager)
    : IRequestHandler<GetUserClaimsQuery, IReadOnlyList<ClaimDto>>
{
    public async Task<IReadOnlyList<ClaimDto>> Handle(GetUserClaimsQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var claims = await userManager.GetClaimsAsync(user);
        return claims.Select(c => new ClaimDto(c.Type, c.Value)).ToList();
    }
}
