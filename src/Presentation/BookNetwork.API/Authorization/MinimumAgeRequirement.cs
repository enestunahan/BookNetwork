using Microsoft.AspNetCore.Authorization;

namespace BookNetwork.API.Authorization;

public sealed class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}
