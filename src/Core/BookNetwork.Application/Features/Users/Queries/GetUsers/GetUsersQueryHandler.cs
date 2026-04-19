using BookNetwork.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(UserManager<AppUser> userManager)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    public async Task<IReadOnlyList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserListItemDto(u.Id, u.UserName!, u.Email!, u.NameSurname))
            .ToListAsync(cancellationToken);
    }
}
