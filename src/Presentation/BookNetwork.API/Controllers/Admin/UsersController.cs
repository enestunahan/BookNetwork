using BookNetwork.API.Authorization;
using BookNetwork.Application.Features.Auth.Commands.Register;
using BookNetwork.Application.Features.Users.Commands.AssignClaim;
using BookNetwork.Application.Features.Users.Queries.GetUserClaims;
using BookNetwork.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthPolicies.MinAdmin)]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RegisterResponse>> CreateUser(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{userId}/claims")]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClaimDto>>> GetUserClaims(
        string userId,
        CancellationToken cancellationToken)
    {
        var claims = await sender.Send(new GetUserClaimsQuery(userId), cancellationToken);
        return Ok(claims);
    }

    [HttpPost("{userId}/claims")]
    public async Task<IActionResult> AssignClaim(
        string userId,
        [FromBody] AssignClaimBody body,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AssignClaimToUserCommand(userId, body.ClaimType, body.ClaimValue), cancellationToken);
        return NoContent();
    }

    public sealed record AssignClaimBody(string ClaimType, string ClaimValue);
}
