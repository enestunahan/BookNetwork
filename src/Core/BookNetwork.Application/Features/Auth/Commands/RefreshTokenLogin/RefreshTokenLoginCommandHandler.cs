using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Application.Common.Security;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Application.Features.Auth.Commands.RefreshTokenLogin;

public sealed class RefreshTokenLoginCommandHandler(
    UserManager<AppUser> userManager,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenLoginCommand, TokenDto>
{
    public async Task<TokenDto> Handle(RefreshTokenLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user is null)
            throw new AuthenticationFailedException("Geçersiz refresh token.");

        if (user.RefreshTokenEndDate is null || user.RefreshTokenEndDate < DateTime.UtcNow)
            throw new AuthenticationFailedException("Refresh token süresi dolmuş. Lütfen yeniden giriş yapın.");

        var token = await tokenService.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenEndDate = token.RefreshTokenExpiration;
        await userManager.UpdateAsync(user);

        return token;
    }
}
