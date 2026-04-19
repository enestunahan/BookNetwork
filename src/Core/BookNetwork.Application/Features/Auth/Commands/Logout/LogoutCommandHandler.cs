using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(UserManager<AppUser> userManager)
    : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        user.RefreshToken = null;
        user.RefreshTokenEndDate = null;
        await userManager.UpdateAsync(user);

        return Unit.Value;
    }
}
