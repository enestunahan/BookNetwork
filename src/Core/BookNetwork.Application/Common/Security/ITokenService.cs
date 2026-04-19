using BookNetwork.Domain.Entities.Identity;

namespace BookNetwork.Application.Common.Security;

public interface ITokenService
{
    Task<TokenDto> CreateTokenAsync(AppUser user);
    string CreateRefreshToken();
}
