using BookNetwork.API.Authorization;
using BookNetwork.Application.Features.Roles.Commands.AssignRoleToUser;
using BookNetwork.Application.Features.Roles.Commands.CreateRole;
using BookNetwork.Application.Features.Roles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = AuthPolicies.MinEditor)]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.MinAdmin)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await sender.Send(new GetRolesQuery(), cancellationToken);
        return Ok(roles);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.MinAdmin)]
    [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateRoleResponse>> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("assign-to-user")]
    public async Task<IActionResult> AssignToUser(
        [FromBody] AssignRoleToUserCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }
}
