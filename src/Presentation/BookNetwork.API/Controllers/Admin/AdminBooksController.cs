using BookNetwork.API.Authorization;
using BookNetwork.Application.Features.Books.Queries.GetBooksForAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers.Admin;

[ApiController]
[Route("api/adminBooks")]
[Authorize(Policy = AuthPolicies.MinEditor)]
public sealed class AdminBooksController(ISender sender) : ControllerBase
{
    [HttpGet("books")]
    [ProducesResponseType(typeof(GetBooksForAdminQueryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetBooksForAdminQueryResponse>> GetBooksForAdmin(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetBooksForAdminQueryRequest(), cancellationToken);
        return Ok(response);
    }
}
