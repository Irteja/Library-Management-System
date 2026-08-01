using LibraryManagementSystem.Application.Books.Commands.BorrowBook;
using LibraryManagementSystem.Application.Books.Commands.ReturnBook;
using LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;
using LibraryManagementSystem.Application.Loans.Queries.GetMemberLoans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoansController(IMediator mediator)
    {
        _mediator = mediator;
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

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetActiveLoans()
    {
        var loans = await _mediator.Send(new GetActiveLoansQuery());
        return Ok(loans);
    }

    [HttpGet("member/{memberId:guid}")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> GetMemberHistory(Guid memberId)
    {
        var loans = await _mediator.Send(new GetMemberLoansQuery(memberId));
        return Ok(loans);
    }
}
