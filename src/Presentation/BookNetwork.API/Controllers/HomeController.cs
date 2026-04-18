using BookNetwork.Application.Features.Home.Queries.GetHomePageBooks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookNetwork.API.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomeController(ISender sender) : ControllerBase
{
    [HttpGet("books")]
    // todo bu alt satırdaki kod ne işe yarıyor araştır bakalım
    [ProducesResponseType(typeof(IReadOnlyList<HomePageBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HomePageBookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await sender.Send(new GetHomePageBooksQuery(), cancellationToken);

        return Ok(books);
    }
}
