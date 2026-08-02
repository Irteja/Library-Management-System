using LibraryManagementSystem.Application.Librarians.Commands.CreateLibrarian;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LibrariansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibrariansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLibrarianCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var librarians = await _mediator.Send(new LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians.GetAllLibrariansQuery(search, page, size));
        return Ok(librarians);
    }
}
