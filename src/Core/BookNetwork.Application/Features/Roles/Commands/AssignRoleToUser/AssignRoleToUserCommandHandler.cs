using BookNetwork.Application.Common.Authorization;
using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Application.Common.Security;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Roles.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler(
    UserManager<AppUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<AssignRoleToUserCommand, Unit>
{
    public async Task<Unit> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var isAdmin = currentUser.IsInRole(AppRoles.Admin);
        var assigningAdmin = request.RoleNames.Any(r =>
            string.Equals(r, AppRoles.Admin, StringComparison.OrdinalIgnoreCase));

        if (assigningAdmin && !isAdmin)
            throw new BusinessException("Admin rolünü yalnızca Admin yetkisine sahip kullanıcılar atayabilir.");

        var user = await userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var currentRoles = await userManager.GetRolesAsync(user);

        if (!isAdmin && currentRoles.Contains(AppRoles.Admin))
            throw new BusinessException("Admin kullanıcıların rolünü yalnızca Admin değiştirebilir.");

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            throw new BusinessException("Kullanıcının mevcut rolleri kaldırılamadı.");

        var addResult = await userManager.AddToRolesAsync(user, request.RoleNames);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(" | ", addResult.Errors.Select(e => e.Description));
            throw new BusinessException($"Rol ataması başarısız: {errors}");
        }

        return Unit.Value;
    }
}
