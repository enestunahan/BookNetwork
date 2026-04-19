using BookNetwork.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BookNetwork.Persistence.Seed;

public static class IdentitySeeder
{
    public const string DefaultPassword = "Password123!";

    private static readonly string[] Roles = ["Admin", "Editor", "User", "Viewer"];

    private static readonly SeedUser[] Users =
    [
        new("admin", "admin@booknetwork.local", "Admin", "Admin", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
        new("enes.editor", "enes.editor@booknetwork.local", "Enes Editor Çömez", "Editor", new DateTime(1992, 5, 10, 0, 0, 0, DateTimeKind.Utc)),
        new("enes.user", "enes.user@booknetwork.local", "Enes User Çömez", "User", new DateTime(2000, 3, 15, 0, 0, 0, DateTimeKind.Utc)),
        new("kucuk.user", "kucuk.user@booknetwork.local", "Küçük User", "User", new DateTime(2014, 8, 5, 0, 0, 0, DateTimeKind.Utc)),
        new("enes.viewer", "enes.viewer@booknetwork.local", "Enes Viewer Çömez", "Viewer", new DateTime(1995, 2, 20, 0, 0, 0, DateTimeKind.Utc))
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName
            });
        }

        foreach (var seed in Users)
        {
            var existing = await userManager.FindByNameAsync(seed.UserName);
            if (existing is null)
            {
                var user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = seed.UserName,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    NameSurname = seed.NameSurname,
                    BirthDate = seed.BirthDate
                };

                var result = await userManager.CreateAsync(user, DefaultPassword);
                if (!result.Succeeded)
                    continue;

                await userManager.AddToRoleAsync(user, seed.Role);
            }
            else
            {
                var changed = false;
                if (existing.BirthDate != seed.BirthDate)
                {
                    existing.BirthDate = seed.BirthDate;
                    changed = true;
                }

                if (changed)
                    await userManager.UpdateAsync(existing);

                if (!await userManager.IsInRoleAsync(existing, seed.Role))
                    await userManager.AddToRoleAsync(existing, seed.Role);
            }
        }
    }

    private sealed record SeedUser(
        string UserName,
        string Email,
        string NameSurname,
        string Role,
        DateTime BirthDate);
}
