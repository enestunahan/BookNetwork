using Microsoft.AspNetCore.Identity;

namespace BookNetwork.Domain.Entities.Identity;

public class AppRole : IdentityRole<string>
{
    public ICollection<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
}
