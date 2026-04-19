using System.Security.Claims;
using BookNetwork.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers;

[ApiController]
[Route("api/examples")]
public sealed class ExamplesController : ControllerBase
{
    [HttpGet("members-only")]
    [Authorize(Policy = AuthPolicies.MinUser)]
    public IActionResult MembersOnly()
    {
        return Ok(new
        {
            message = "Üye alanına hoş geldin!",
            user = User.Identity?.Name,
            roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        });
    }

    [HttpGet("adults-only")]
    [Authorize(Policy = AuthPolicies.Adult)]
    public IActionResult AdultsOnly()
    {
        return Ok(new
        {
            message = "18+ içerik: sadece yetişkin üyeler için.",
            user = User.Identity?.Name,
            birthDate = User.FindFirstValue(ClaimTypes.DateOfBirth)
        });
    }
}
