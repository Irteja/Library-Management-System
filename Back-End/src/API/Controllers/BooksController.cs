using LibraryManagementSystem.Application.Books.Commands;
using LibraryManagementSystem.Application.Books.Commands.DeleteBook;
using LibraryManagementSystem.Application.Books.Commands.UpdateBook;
using LibraryManagementSystem.Application.Books.Queries.GetAllBooks;
using LibraryManagementSystem.Application.Books.Queries.GetBookById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var books = await _mediator.Send(new GetAllBooksQuery(search, page, size));
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _mediator.Send(new GetBookByIdQuery(id));
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create(CreateBookCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Update(Guid id, UpdateBookCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBookCommand(id));
        return NoContent();
    }

}
