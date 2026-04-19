using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Application.Features.Roles.Queries.GetRoles;

public sealed class GetRolesQueryHandler(RoleManager<AppRole> roleManager)
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    public async Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await roleManager.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(r.Id, r.Name!))
            .ToListAsync(cancellationToken);
    }
}
