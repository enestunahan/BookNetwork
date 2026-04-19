using System.Security.Claims;
using BookNetwork.Application.Common.Security;
using BookNetwork.Application.Features.Auth.Commands.Login;
using BookNetwork.Application.Features.Auth.Commands.Logout;
using BookNetwork.Application.Features.Auth.Commands.RefreshTokenLogin;
using BookNetwork.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers;

// TODO(dynamic-authz): DB-driven RBAC / endpoint-bazlı yetkilendirmeye geçiş.
//  Şu an policy'ler statik ([Authorize(Policy = AuthPolicies.MinX)]).
//  Planlanan akış (karar verildiğinde uygulanacak):
//   1) SyncEndpointsCommand: reflection ile tüm controller+action'ları Endpoints tablosuna yaz (Code = "Auth.Login" vs.).
//   2) Admin panel: RoleEndpoints eşlemesi için CRUD endpointleri (POST/DELETE /api/admin/roles/{roleId}/endpoints).
//   3) Custom IAuthorizationPolicyProvider + DynamicPermissionHandler: [Authorize("Auth.Login")] gördüğünde DB'den role-endpoint eşleşmesini okusun.
//   4) IMemoryCache ile role-endpoint map'ini cache'le, rol/endpoint değişiminde invalidate et.
//   5) "Admin" rolü super-admin: her zaman pass (DB check atla).
//   6) Mevcut statik [Authorize(Policy = AuthPolicies.MinX)] kullanımlarını kademeli olarak endpoint code'una taşı.
//  Detaylar: docs/authorization.md §10.

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenDto>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(command, cancellationToken);
        return Ok(token);
    }

    [HttpPost("refresh-token-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenDto>> RefreshTokenLogin(
        [FromBody] RefreshTokenLoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(command, cancellationToken);
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException();

        await sender.Send(new LogoutCommand(userId), cancellationToken);
        return NoContent();
    }
}
