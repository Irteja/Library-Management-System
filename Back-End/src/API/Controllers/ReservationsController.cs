using LibraryManagementSystem.Application.Books.Commands.ReserveBook;
using LibraryManagementSystem.Application.Reservations.Commands.CancelReservation;
using LibraryManagementSystem.Application.Reservations.Queries.GetBookReservationQueue;
using LibraryManagementSystem.Application.Reservations.Queries.GetMemberReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> Create(ReserveBookCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok($"Reservation {id}");
    }

    [HttpGet("member/{memberId:guid}")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> GetMemberReservations(Guid memberId)
    {
        var reservations = await _mediator.Send(new GetMemberReservationsQuery(memberId));
        return Ok(reservations);
    }

    [HttpGet("book/{bookId:guid}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetBookQueue(Guid bookId)
    {
        var queue = await _mediator.Send(new GetBookReservationQueueQuery(bookId));
        return Ok(queue);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _mediator.Send(new CancelReservationCommand(id));
        return NoContent();
    }
}
