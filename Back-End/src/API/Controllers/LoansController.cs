using LibraryManagementSystem.Application.Books.Commands.BorrowBook;
using LibraryManagementSystem.Application.Books.Commands.ReturnBook;
using LibraryManagementSystem.Application.Common.Services;
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
    private readonly IMemberAccessService _memberAccess;

    public LoansController(IMediator mediator, IMemberAccessService memberAccess)
    {
        _mediator = mediator;
        _memberAccess = memberAccess;
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

    [HttpGet("my")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> GetMyLoans(CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetCurrentMemberIdAsync(cancellationToken);
        var loans = await _mediator.Send(new GetMemberLoansQuery(memberId!.Value), cancellationToken);
        return Ok(loans);
    }
}
