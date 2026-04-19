using BookNetwork.Application.Common.Exceptions;
using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Application.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandHandler(RoleManager<AppRole> roleManager)
    : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await roleManager.RoleExistsAsync(request.Name))
            throw new BusinessException($"'{request.Name}' rolü zaten mevcut.");

        var role = new AppRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Rol oluşturulamadı: {errors}");
        }

        return new CreateRoleResponse(role.Id, role.Name!);
    }
}
