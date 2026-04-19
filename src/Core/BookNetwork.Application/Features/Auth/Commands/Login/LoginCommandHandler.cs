using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Application.Common.Security;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    UserManager<AppUser> userManager,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, TokenDto>
{
    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UserNameOrEmail)
                   ?? await userManager.FindByEmailAsync(request.UserNameOrEmail);

        if (user is null)
            throw new AuthenticationFailedException("Kullanıcı adı veya şifre hatalı.");

        if (await userManager.IsLockedOutAsync(user))
            throw new AuthenticationFailedException("Hesap geçici olarak kilitlendi. Lütfen birkaç dakika sonra tekrar deneyin.");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(user);
            throw new AuthenticationFailedException("Kullanıcı adı veya şifre hatalı.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var token = await tokenService.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenEndDate = token.RefreshTokenExpiration;
        await userManager.UpdateAsync(user);

        return token;
    }
}
