using LibraryManagementSystem.Application.Books.Commands.BorrowBook;
using LibraryManagementSystem.Application.Books.Commands;
using LibraryManagementSystem.Application.Books.Commands.ReserveBook;
using LibraryManagementSystem.Application.Books.Commands.ReturnBook;
using LibraryManagementSystem.Domain.Enums;
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
    public IActionResult GetAll()
    {
        return Ok("List all books");
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok($"Book {id}");
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
    public IActionResult Update(Guid id)
    {
        return Ok($"Book {id} updated");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(Guid id)
    {
        return Ok($"Book {id} deleted");
    }

    [HttpPost("borrow")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> Borrow(BorrowBookCommand command)
    {
        var loanId = await _mediator.Send(command);
        return Ok(new { LoanId = loanId });
    }

    [HttpPost("return")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Return(ReturnBookCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("reserve")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> Reserve(ReserveBookCommand command)
    {
        var reservationId = await _mediator.Send(command);
        return Ok(new { ReservationId = reservationId });
    }
}