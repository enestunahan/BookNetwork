using BookNetwork.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var roleResult = await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName
            });

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Rol oluşturulamadı ({roleName}): {errors}");
            }

            logger.LogInformation("Seed: rol oluşturuldu → {Role}", roleName);
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

                var createResult = await userManager.CreateAsync(user, DefaultPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(" | ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Kullanıcı oluşturulamadı ({seed.UserName}): {errors}");
                }

                var roleAssign = await userManager.AddToRoleAsync(user, seed.Role);
                if (!roleAssign.Succeeded)
                {
                    var errors = string.Join(" | ", roleAssign.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Rol ataması başarısız ({seed.UserName} → {seed.Role}): {errors}");
                }

                logger.LogInformation("Seed: kullanıcı oluşturuldu → {UserName} ({Role})", seed.UserName, seed.Role);
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
                {
                    var updateResult = await userManager.UpdateAsync(existing);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(" | ", updateResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Kullanıcı güncellenemedi ({seed.UserName}): {errors}");
                    }
                }

                if (!await userManager.IsInRoleAsync(existing, seed.Role))
                {
                    var roleAssign = await userManager.AddToRoleAsync(existing, seed.Role);
                    if (!roleAssign.Succeeded)
                    {
                        var errors = string.Join(" | ", roleAssign.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Rol ataması başarısız ({seed.UserName} → {seed.Role}): {errors}");
                    }
                }
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
