using BookNetwork.Domain.Common;

namespace BookNetwork.Domain.Entities.Identity;

public sealed class Endpoint : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string HttpType { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string Menu { get; set; } = string.Empty;

    public ICollection<AppRole> Roles { get; set; } = new List<AppRole>();
}
