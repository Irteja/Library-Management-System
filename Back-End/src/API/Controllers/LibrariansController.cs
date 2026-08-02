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
}
