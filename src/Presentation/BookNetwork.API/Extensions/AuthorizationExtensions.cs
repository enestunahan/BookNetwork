using BookNetwork.API.Authorization;
using BookNetwork.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BookNetwork.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.MinAdmin, policy =>
                policy.RequireRole(AppRoles.Admin));

            options.AddPolicy(AuthPolicies.MinEditor, policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.Editor));

            options.AddPolicy(AuthPolicies.MinUser, policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.Editor, AppRoles.User));

            options.AddPolicy(AuthPolicies.MinViewer, policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.Editor, AppRoles.User, AppRoles.Viewer));

            options.AddPolicy(AuthPolicies.Adult, policy =>
            {
                policy.RequireRole(AppRoles.Admin, AppRoles.Editor, AppRoles.User);
                policy.AddRequirements(new MinimumAgeRequirement(18));
            });
        });

        return services;
    }
}
