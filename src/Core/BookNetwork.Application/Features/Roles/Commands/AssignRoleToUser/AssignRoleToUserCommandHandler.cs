using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Roles.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler(UserManager<AppUser> userManager)
    : IRequestHandler<AssignRoleToUserCommand, Unit>
{
    public async Task<Unit> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var currentRoles = await userManager.GetRolesAsync(user);
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
