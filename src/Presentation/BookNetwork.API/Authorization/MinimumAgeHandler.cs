using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BookNetwork.API.Authorization;

public sealed class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var birthClaim = context.User.FindFirst(ClaimTypes.DateOfBirth)?.Value;
        if (string.IsNullOrWhiteSpace(birthClaim))
            return Task.CompletedTask;

        if (!DateTime.TryParse(
                birthClaim,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var birthDate))
            return Task.CompletedTask;

        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;

        if (age >= requirement.MinimumAge)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
