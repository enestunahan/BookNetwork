using System.Security.Claims;
using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Users.Commands.AssignClaim;

public sealed class AssignClaimToUserCommandHandler(UserManager<AppUser> userManager)
    : IRequestHandler<AssignClaimToUserCommand, Unit>
{
    public async Task<Unit> Handle(AssignClaimToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var existing = await userManager.GetClaimsAsync(user);
        if (existing.Any(c => c.Type == request.ClaimType && c.Value == request.ClaimValue))
            throw new BusinessException("Bu claim zaten kullanıcıya tanımlı.");

        var result = await userManager.AddClaimAsync(user, new Claim(request.ClaimType, request.ClaimValue));
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Claim eklenemedi: {errors}");
        }

        return Unit.Value;
    }
}
